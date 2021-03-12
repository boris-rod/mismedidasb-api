using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.BasicResponses;
using MismeAPI.Common.DTO.Menu;
using MismeAPI.Common.DTO.Request;
using MismeAPI.Common.DTO.Response;
using MismeAPI.Middlewares.Security;
using MismeAPI.Service;
using MismeAPI.Service.Utils;
using MismeAPI.Services;
using MismeAPI.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MismeAPI.Controllers
{
    [Route("api/menu")]
    public class MenuController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IMenuService _menuService;
        private readonly IUserService _userService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IGroupService _groupService;

        public MenuController(IMapper mapper, IMenuService menuService, IUserService userService, IAuthorizationService authorizationService, IGroupService groupService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        }

        /// <summary>
        /// Get all menues. Requires authentication.
        /// </summary>
        /// <param name="page">The page to be displayed. 1 by default.</param>
        /// <param name="perPage">The number of eats to be displayed per page. 10 by default.</param>
        /// <param name="groupId">Filter menues per group</param>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<MenuResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Index(int? page, int? perPage, int? groupId)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);

            if (groupId.HasValue)
            {
                var group = await _groupService.GetGroupAsync(groupId.Value);

                // Resource permision handler
                var authorizationResult = await _authorizationService.AuthorizeAsync(User, group, Operations.Read);
                if (!authorizationResult.Succeeded)
                {
                    return Forbid();
                }
                // Resource permission handler
            }

            var pag = page ?? 1;
            var perPag = perPage ?? 10;

            var result = await _menuService.GetMenuesAsync(groupId, pag, perPag, true, loggedUser);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<IEnumerable<MenuResponse>>(result, opt =>
            {
                opt.Items["lang"] = language;
            });
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get all menues. Requires authentication. Only admin can access
        /// </summary>
        /// <param name="page">The page to be displayed. 1 by default.</param>
        /// <param name="perPage">The number of eats to be displayed per page. 10 by default.</param>
        /// <param name="groupId">Filter menues per group</param>
        /// <param name="isActive">Filter menues by active status</param>
        [HttpGet("admin")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(IEnumerable<MenuResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllAdmin(int? page, int? perPage, int? groupId, bool? isActive)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);
            var pag = page ?? 1;
            var perPag = perPage ?? 10;

            var result = await _menuService.GetMenuesAsync(groupId, pag, perPag, isActive, loggedUser);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<IEnumerable<MenuResponse>>(result, opt =>
            {
                opt.Items["lang"] = language;
            });
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get menues of a group. Requires authentication. Only admin and group Admins can access
        /// </summary>
        /// <param name="page">The page to be displayed. 1 by default.</param>
        /// <param name="perPage">The number of eats to be displayed per page. 10 by default.</param>
        /// <param name="id">Filter menues per group</param>
        /// <param name="isActive">Filter menues by active status</param>
        [HttpGet("group/{id}")]
        [Authorize(Roles = "ADMIN,GROUP_ADMIN")]
        [ProducesResponseType(typeof(IEnumerable<MenuResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetGroupMenues([FromRoute] int id, int? page, int? perPage, bool? isActive)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);
            var group = await _groupService.GetCurrentUserGroupAsync(loggedUser);

            // Resource permision handler
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, group, Operations.Update);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            var pag = page ?? 1;
            var perPag = perPage ?? 10;

            var result = await _menuService.GetMenuesAsync(id, pag, perPag, isActive, loggedUser);

            HttpContext.Response.Headers.Add("PagingData", JsonConvert.SerializeObject(result.GetPaginationData));
            HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "PagingData";
            HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "PagingData";

            var mapped = _mapper.Map<IEnumerable<MenuResponse>>(result, opt =>
            {
                opt.Items["lang"] = language;
            });
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Get a menu. Requires authentication. Only admin and group Admins can access
        /// </summary>
        /// <param name="id">The id of the menu to upload.</param>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(MenuResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMenu([FromRoute] int id)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);
            var menu = await _menuService.GetMenuAsync(id);

            // Resource permision handler
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, menu, Operations.Read);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            var mapped = _mapper.Map<IEnumerable<MenuResponse>>(menu, opt =>
            {
                opt.Items["lang"] = language;
            });
            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Create a menu. Requires authentication.
        /// </summary>
        /// <param name="request">Menu request object.</param>
        [HttpPost]
        [Authorize(Roles = "ADMIN,GROUP_ADMIN")]
        [ProducesResponseType(typeof(MenuResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddMenu([FromBody] CreateUpdateMenuRequest request)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);

            var result = await _menuService.CreateMenuAsync(request, loggedUser);

            var mapped = _mapper.Map<MenuResponse>(result, opt =>
            {
                opt.Items["lang"] = language;
            });

            return Created("", new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Update a menu. Requires authentication.
        /// </summary>
        /// <param name="id">Id of the menu to edit</param>
        /// <param name="request">Menu request object.</param>
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,GROUP_ADMIN")]
        [ProducesResponseType(typeof(MenuResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateMenu([FromRoute] int id, [FromBody] CreateUpdateMenuRequest request)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);
            var menu = await _menuService.GetMenuAsync(id);

            // Resource permision handler
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, menu, Operations.Update);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            menu = await _menuService.UpdateMenuAsync(id, request);

            var mapped = _mapper.Map<MenuResponse>(menu, opt =>
            {
                opt.Items["lang"] = language;
            });

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Active a menu. Requires authentication.
        /// </summary>
        /// <param name="id">Id of the menu to edit</param>
        [HttpPatch("{id}/active")]
        [Authorize(Roles = "ADMIN,GROUP_ADMIN")]
        [ProducesResponseType(typeof(MenuResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ActiveMenu([FromRoute] int id)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);
            var menu = await _menuService.GetMenuAsync(id);

            // Resource permision handler
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, menu, Operations.Update);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            menu = await _menuService.PatchMenuStatusAsync(id, true);

            var mapped = _mapper.Map<MenuResponse>(menu, opt =>
            {
                opt.Items["lang"] = language;
            });

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Active a menu. Requires authentication.
        /// </summary>
        /// <param name="id">Id of the menu to edit</param>
        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = "ADMIN,GROUP_ADMIN")]
        [ProducesResponseType(typeof(MenuResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeactivateMenu([FromRoute] int id)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);
            var menu = await _menuService.GetMenuAsync(id);

            // Resource permision handler
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, menu, Operations.Update);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            menu = await _menuService.PatchMenuStatusAsync(id, false);

            var mapped = _mapper.Map<MenuResponse>(menu, opt =>
            {
                opt.Items["lang"] = language;
            });

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Bulk update menu eats. Requires authentication.
        /// </summary>
        /// <param name="id">Id of the menu to edit</param>
        /// <param name="request">Update eats request object</param>
        [HttpPost("{id}/update-eats")]
        [Authorize(Roles = "ADMIN,GROUP_ADMIN")]
        [ProducesResponseType(typeof(MenuResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> BulkEatsUpdate([FromRoute] int id, [FromBody] MenuBulkEatsUpdateRequest request)
        {
            var loggedUser = User.GetUserIdFromToken();
            var language = await _userService.GetUserLanguageFromUserIdAsync(loggedUser);
            var menu = await _menuService.GetMenuAsync(id);

            // Resource permision handler
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, menu, Operations.Update);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            menu = await _menuService.UpdateBulkMenuEatsAsync(id, request);

            var mapped = _mapper.Map<MenuResponse>(menu, opt =>
            {
                opt.Items["lang"] = language;
            });

            return Ok(new ApiOkResponse(mapped));
        }

        /// <summary>
        /// Delete a menu. Requires authentication.
        /// </summary>
        /// <param name="id">Id of the menu to edit</param>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN,GROUP_ADMIN")]
        [ProducesResponseType(typeof(MenuResponse), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ApiResponse), (int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> DeleteMenu([FromRoute] int id)
        {
            var loggedUser = User.GetUserIdFromToken();
            var menu = await _menuService.GetMenuAsync(id);

            // Resource permision handler
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, menu, Operations.Delete);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            // Resource permission handler

            await _menuService.DeleteAsync(id);

            return NoContent();
        }
    }
}
