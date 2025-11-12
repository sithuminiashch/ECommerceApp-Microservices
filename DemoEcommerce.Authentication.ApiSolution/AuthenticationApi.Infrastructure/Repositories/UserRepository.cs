using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthenticationApi.Application.DTOs;
using eCommerce.SharedLibrary.Responses;
using AuthenticationApi.Application.Interfaces;
using AuthenticationApi.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using AuthenticationApi.Domain.Entities;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using AuthenticationApi.Application.Interfaces;

namespace AuthenticationApi.Infrastructure.Repositories
{
    internal class UserRepository(AuthenticationDbContext context,IConfiguration config):IUser
    {

        private async Task<AppUser> GetUserByEmail(string email)
        {
            var user=await context.Users.FirstOrDefaultAsync(u=>u.Email==email);
            return user is null ? null! : user!;

        }
       
        public async Task<GetUserDTO> GetUser(int userId)
        {
            var user = await context.Users.FindAsync(userId);
            return user is not null ? new GetUserDTO(
                user.Id,
                user.Name!,
                user.TelephoneNumber!,
                user.Email!,
                user.Role!,
                user.Address!
                ) : null!;

        }


        private string GenerateToken(AppUser user)
        {
            var key = Encoding.UTF8.GetBytes(config.GetSection("Authentication:Key").Value!);
            var securityKey=new SymmetricSecurityKey(key);
            var credentials=new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name,user.Name!),
                new(ClaimTypes.Email,user.Email!)
            };
            if (!string.IsNullOrEmpty(user.Role) || !Equals("string", user.Role))
                claims.Add(new(ClaimTypes.Role, user.Role!));


            var token = new JwtSecurityToken(
                issuer: config["Authentication:Issuer"],
                audience: config["Authentication:Audience"],
                claims: claims,
                expires: null,
                signingCredentials:credentials

                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
     
        public async Task<Response> Login(LoginDTO loginDTO)
        {
            var getUser=await GetUserByEmail(loginDTO.Email);
            if (getUser is null)
                return new Response(false, "Invalid credentials");

            bool verifyPassword = BCrypt.Net.BCrypt.Verify(loginDTO.Password, getUser.Password);
            if (!verifyPassword)
                return new Response(false, "Invalid credentials");

            string token = GenerateToken(getUser);
            return new Response(true,token);

        }


        public async Task<Response> Register(AppUserDTO appUserDTO)
        {
            var getuser=await GetUserByEmail(appUserDTO.Email);
            if (getuser is not null)
                return new Response(false, "$You Cannot Use This Email For Registration");

            var result = context.Users.Add(new AppUser()
            {
                Name = appUserDTO.Name,
                Email = appUserDTO.Email,
                TelephoneNumber = appUserDTO.TelephoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(appUserDTO.Password),
                Address = appUserDTO.Address,
                Role = appUserDTO.Role
            });

            await context.SaveChangesAsync();
            return result.Entity.Id > 0 ? new Response(true, "User Registered Successfully") :
                new Response(false, "Invalid data Provided");


        }
    }
}
