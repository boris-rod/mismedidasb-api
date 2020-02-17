using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MismeAPI.Service;
using System;

namespace MismeAPI.Controllers
{
    [Route("api/dish")]
    public class DishController : Controller
    {
        private readonly IDishService _dishService;
        private readonly IMapper _mapper;

        public DishController(IDishService dishService, IMapper mapper)
        {
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
    }
}