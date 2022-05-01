﻿using System;
using System.Collections.Generic;
using GamesWebApp.Authorization;
using GamesWebApp.Data;
using GamesWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GamesWebApp.Pages.Games
{
    [AllowAnonymous]
    public class IndexModel : DI_BasePageModel
    {
        public IndexModel(
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
        }

        public IList<Game> Game { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public SelectList Categories { get; set; }
        [BindProperty(SupportsGet = true)]
        public string GameCategory { get; set; }

     
        //[HttpGet]
        //[Route("/Games/Search/{min}/{max}")]

        public async Task OnGetAsync()
        {
            IQueryable<string> genreQuery = (IQueryable<string>)(from m in Context.Game
                                            orderby m.Category
                                            select m.Category);

            var isAuthorized = User.IsInRole(Constants.AdministratorsRole);

            var currentUserId = UserManager.GetUserId(User);

            var games = Context.Game
                 .Include(c => c.Platform)
                 .Include(c => c.ApplicationUser).AsQueryable();

            // var users = UserManager.Users.
            // Include(d => d.UserName);
            if (!string.IsNullOrEmpty(SearchString))
            {
                games = games.Where(s => s.Name.Contains(SearchString));
            }

            if (!string.IsNullOrEmpty(GameCategory))
            {
                games = games.Where(x => x.Category == GameCategory);
            }


            // Only approved games are shown UNLESS you're authorized to see them
            // or you are the owner.
            if (!isAuthorized)
            {
                games = games.Where(c => c.Status == Models.Game.GameStatus.Approved
                                            || c.OwnerID == currentUserId);
            }

            Categories = new SelectList(await genreQuery.Distinct().ToListAsync());
            Game = await games.ToListAsync();
        }
    }
}