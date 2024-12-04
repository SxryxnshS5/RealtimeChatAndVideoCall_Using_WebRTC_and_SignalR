//namespace WebRTCWebApp.Server.Controllers {
//    using Microsoft.AspNetCore.Http;
//    using Microsoft.AspNetCore.Mvc;
//    using WebRTCWebApp.Server.Services;

//    [ApiController]
//    [Route("api/[controller]")]
//    public class InvitationCodeController : ControllerBase {
//        private readonly InvitationCodeService _invitationCodeService;

//        public InvitationCodeController(InvitationCodeService invitationCodeService) {
//            _invitationCodeService = invitationCodeService;
//        }

//        [HttpPost("Add")]
//        public IActionResult AddCode([FromBody] string code) {
//            _invitationCodeService.AddCode(code);
//            return Ok();
//        }

//        [HttpGet("Validate/{code}")]
//        public IActionResult ValidateCode(string code) {
//            bool isValid = _invitationCodeService.IsValidCode(code);
//            return Ok(isValid);
//        }
//    }

//}
