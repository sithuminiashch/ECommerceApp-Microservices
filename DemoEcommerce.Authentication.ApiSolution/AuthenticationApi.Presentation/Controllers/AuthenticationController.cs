using AuthenticationApi.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using eCommerce.SharedLibrary.Responses;
using AuthenticationApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace AuthenticationApi.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthenticationController (IUser userInterface): ControllerBase
    {

        [HttpPost("register")]
        public async Task<ActionResult<Response>> Register(AppUserDTO appuserDTO)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            var result= await userInterface.Register(appuserDTO);
            return result.Flag ? Ok(result) : BadRequest(result);
            
        }

        [HttpPost("login")]
        public async Task<ActionResult<Response>> Login(LoginDTO loginDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await userInterface.Login(loginDTO);
            return result.Flag ? Ok(result) : BadRequest(result);


        }
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<GetUserDTO>> GetUser(int id)
        {
            if (id<=0) return BadRequest("Invalid UserId");

            var user = await userInterface.GetUser(id);
            return user.Id>0 ? Ok(user) :NotFound(user);


        }

    }
}
