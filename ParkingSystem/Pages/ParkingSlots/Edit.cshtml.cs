﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Models;


namespace ParkingSystem.Pages.ParkingSlots
{
    public class EditModel : PageModel
    {

        public SelectList CustomerSL { get; set; }

       
        public SelectList updater { get; set; }
        public void CustomerDropdown(ParkingSystemContext _context, object selectedCustomer = null)
        {
            var customerQuery = from d in _context.Customer
                                orderby d.CustomerID
                                select new SelectListItem
                                {
                                    Text = d.FirstName + " " + d.LastName,
                                    Value = d.CustomerID.ToString()
                                }; ;

            CustomerSL = new SelectList(customerQuery, "Value", "Text", selectedCustomer);
        }
        private readonly ParkingSystem.Models.ParkingSystemContext _context;


        public EditModel(ParkingSystem.Models.ParkingSystemContext context)
        {

            _context = context;
        }

        [BindProperty]
        public ParkingSlot ParkingSlot { get; set; }

        [BindProperty]
        public Payment Payment { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CustomerDropdown(_context);
            ParkingSlot = await _context.ParkingSlot.FirstOrDefaultAsync(m => m.ParkingID == id);
            var getReceipt = (from h in _context.Payment where h.ParkingID == id select h.ReceiptNo).Sum();
            Payment = await _context.Payment.FirstOrDefaultAsync(m => m.ReceiptNo == getReceipt);
            if (ParkingSlot == null)
            {
                return NotFound();
            }
            return Page();
        }
        private Payment GetPaymentFromDb(int id)
        {
            return _context.Payment.Single(p => p.ReceiptNo == id);
        }
        public void setTotal(decimal tot, int id, double hours)
        {
            Payment PaymentRecord = GetPaymentFromDb(id);

            PaymentRecord.TimeOfPayment = DateTime.Now;
            PaymentRecord.Total = tot;
            PaymentRecord.Period = Convert.ToInt32(hours);
            
                _context.Payment.Attach(PaymentRecord);

                var entry = _context.Entry(PaymentRecord);
                entry.State = EntityState.Modified;

                _context.SaveChanges();
            


        }
        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (ParkingSlot.TimeOut != null)
            {
                TimeSpan? hours = ParkingSlot.TimeOut - ParkingSlot.TimeIn;
                var totalhours = hours.Value.TotalHours;
                decimal final = Convert.ToDecimal(totalhours)+1;
                var getReceipt = (from h in _context.Payment join b in _context.ParkingSlot on h.ParkingID equals b.ParkingID where b.ParkingID == id  select h.ReceiptNo).Sum();
                int receipt = Convert.ToInt32(getReceipt);
                var getCustomer = (from z in _context.Payment join x in _context.ParkingSlot on z.ParkingID equals x.ParkingID select x.CustomerID).Sum();
                if (final > 6) final = 6;
                var getPrice = (from p in _context.Pricing where (p.Period == final) select p.Price).Sum();

                var getCustDiscount = (from e in _context.Customer join t in _context.PositionDiscount on e.Position equals t.Position where (e.CustomerID == getCustomer) select t.Price).Sum();

                final = (1 - (Convert.ToDecimal(getCustDiscount))) * Convert.ToDecimal(getPrice);
                final =  decimal.Round(final, 2, MidpointRounding.AwayFromZero);
                setTotal(final,getReceipt,totalhours);
               
            }
            if (!ModelState.IsValid)
            {
                return Page(); 
            }


            _context.Attach(ParkingSlot).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParkingSlotExists(ParkingSlot.ParkingID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ParkingSlotExists(int id)
        {
            return _context.ParkingSlot.Any(e => e.ParkingID == id);
        }

    }
}
