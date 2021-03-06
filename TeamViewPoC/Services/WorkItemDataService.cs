﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamViewPoC.Data;
using TeamViewPoC.Models;

namespace TeamViewPoC.Services
{
    public class WorkItemDataService : IWorkItemDataService
    {
        private readonly ApplicationDbContext _context;

        //injecting the db into the constructor and setting it to the private field
        public WorkItemDataService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task AddWorkItemAsync(WorkItem item)
        {
            await _context.WorkItems.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<WorkItem>> GetActiveWorkItemsAsync()
        {
            return await _context.WorkItems.Where(x=>x.Complete == false && x.Active == true).ToArrayAsync();
        }

        public async Task<IEnumerable<WorkItem>> GetMyRecentlyCompletedAsync()
        {
            return await _context.WorkItems.Where(x => x.Complete == true && x.Active == true && x.LastUpdated.AddDays(30) >= DateTime.Now).ToArrayAsync();
        }

        public async Task<IEnumerable<WorkItem>> GetMyWorkItemsAsync(string sort)
        {
            if (sort == "due")
            {
                return await _context.WorkItems.Where(x => x.AssignedTo == Constants.HardCodedSignedInUser && x.Complete == false && x.Active == true).OrderBy(x => x.DueDate).Include(x => x.Notes).Include(x => x.Project).ToArrayAsync();
            }

            return await _context.WorkItems.Where(x => x.AssignedTo == Constants.HardCodedSignedInUser && x.Complete == false && x.Active ==true).Include(x => x.Notes).ToArrayAsync();
        }

        public async Task<WorkItem> GetWorkItemByIdAsync(int id)
        {
            return await _context.WorkItems.Include(x => x.Notes).Include(x=>x.Project).FirstOrDefaultAsync(x => x.WorkItemId == id);
        }

        public async Task MarkComplete(int id)
        {
            var update =  await _context.WorkItems.FirstOrDefaultAsync(x => x.WorkItemId == id);
            update.Complete = true;
            update.LastUpdated = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async void UpdateWorkItemAsync(WorkItem item)
        {
            _context.WorkItems.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task Update(WorkItem item)
        {
            _context.WorkItems.Update(item);
            item.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<WorkItem>> GetMyDueSoon(int days)
        {
            return await _context.WorkItems.Where(x => x.Complete == false && x.DueDate < DateTime.Now.AddDays(days) 
            && !(x.DueDate.Date == DateTime.Now.Date || x.DueDate < DateTime.Now.Date && x.Complete == false) && x.Active == true).ToArrayAsync();
        }

        public async Task<IEnumerable<WorkItem>> GetMyDueToday()
        {
            return await _context.WorkItems.Where(x => x.DueDate.Date <= DateTime.Now.Date && x.Complete == false && x.Active==true).ToArrayAsync();
        }

        public async Task SetInactive(WorkItem item)
        {
            _context.WorkItems.Update(item);
            item.Active = false;
            item.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<WorkItem>> GetByProjectId(int id)
        {
            return await _context.WorkItems.Where(x => x.Project.ProjectId == id).OrderBy(x=>x.DueDate).ToArrayAsync();
        }

        public async Task<IEnumerable<WorkItem>> SearchMyWorkItems(string searchstring)
        {
            return await _context.WorkItems.Where(x => x.AssignedTo == Constants.HardCodedSignedInUser
            && x.Complete == false
            && x.Active == true
            && (x.Title.Contains(searchstring) 
                || x.Description.Contains(searchstring) 
                || x.Project.Title.Contains(searchstring)
                || x.Project.Description.Contains(searchstring))
            ).Include(x=>x.Project).ToArrayAsync();
        }
    }
}
