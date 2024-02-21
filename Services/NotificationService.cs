
using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Timers;
using Timer = System.Timers.Timer;

namespace KozoskodoAPI.Services
{
    public class NotificationService
    {
        private readonly DBContext _dbContext;
        private readonly ITimerService _timerService;
        public NotificationService(DBContext context, ITimerService timerService) {
            _dbContext = context;
            _timerService = timerService;

            //DateTime now = DateTime.Now;
            //DateTime nextMidnight = now.Date.AddDays(1);

            _timerService.Elapsed += OnTimedEvent;

            // Indítsd el a timert
            _timerService.Start(TimeSpan.FromDays(1));

            // Számoljuk ki a Timer indulási késleltetését az éjfélhez képest
            //double delayMilliseconds = (nextMidnight - now).TotalMilliseconds;

            //Timer timer = new Timer(delayMilliseconds);
            //timer.Elapsed += OnTimedEvent;
            //timer.AutoReset = true;
            //timer.Enabled = true;
        }


        public async void OnTimedEvent(object sender, EventArgs e)
        {

        }

        //public async void OnTimedEvent(object source, ElapsedEventArgs e)
        //{
        //    DateTime today = DateTime.Today;
        //    var users =  await _dbContext.Personal.Where(u => u.DateOfBirth == DateOnly.FromDateTime(today)).ToListAsync();

        //    foreach (var person in users)
        //    {
        //        if (person.DateOfBirth.Value.Month == today.Month && person.DateOfBirth.Value.Day == today.Day)
        //        {
        //            //TODO: értesítés elküldése az ismerősök számára
        //        }
        //    }


        //    // Állítsa be a következő indulási időpontot (24 óra múlva)
        //    ((Timer)source).Interval = 24 * 60 * 60 * 1000;
        //}
    }
}
