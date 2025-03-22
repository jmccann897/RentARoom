using Microsoft.AspNetCore.SignalR;
using RentARoom.DataAccess.Repository.IRepository;  // For IUnitOfWork
using RentARoom.Models;
using RentARoom.Models.DTOs;  // For PropertyView model


namespace RentARoom.Services.IServices
{
    public class PropertyViewService : IPropertyViewService
    {
        private readonly IUnitOfWork _unitOfWork;



        public PropertyViewService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }

        public IEnumerable<object> GetViewsPerDay(int propertyId)
        {

            if (propertyId <= 0)
            {
                return new List<PropertyViewStatsDTO>(); // Return empty list for invalid ID
            }

            var viewsPerDay = _unitOfWork.PropertyView
                .Find(pv => pv.PropertyId == propertyId)
                .GroupBy(pv => pv.ViewedAt.Date)
                .Select(g => new PropertyViewStatsDTO
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Views = g.Count()
                })
                .OrderBy(result => result.Date)
                .ToList();

            return viewsPerDay;
        }

        public async Task<int> LogPropertyViewAsync(int propertyId)
        {
            if (propertyId <= 0)
            {
                return 0; // Return 0 for invalid ID
            }

            var propertyView = new PropertyView
            {
                PropertyId = propertyId,
                ViewedAt = DateTime.UtcNow
            };

            _unitOfWork.PropertyView.Add(propertyView);
            _unitOfWork.Save();

            int totalViews = _unitOfWork.PropertyView.Find(v => v.PropertyId == propertyId).Count();
            return totalViews;
        }
    }
}
