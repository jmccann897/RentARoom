
namespace RentARoom.Services.IServices
{
    public interface IPropertyViewService
    {
        /// <summary>
        /// Update propertyViews against property Id
        /// </summary>
        /// <param name="propertyId"></param>
        /// <returns></returns>
        Task<int> LogPropertyViewAsync(int propertyId);
        /// <summary>
        /// Return propertyViews for property Id
        /// </summary>
        /// <param name="propertyId"></param>
        /// <returns></returns>
        IEnumerable<object> GetViewsPerDay(int propertyId);
    }

}
