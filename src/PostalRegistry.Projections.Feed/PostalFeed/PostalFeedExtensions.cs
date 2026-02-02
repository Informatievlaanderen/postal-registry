namespace PostalRegistry.Projections.Feed.PostalFeed
{
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Microsoft.EntityFrameworkCore;

    public static class PostalFeedExtensions
    {
        public static async Task<int> CalculatePage(this FeedContext context, int maxPageSize = ChangeFeedService.DefaultMaxPageSize)
        {
            if (!context.PostalFeed.Any())
                return 1;

            var maxPage = await context.PostalFeed.MaxAsync(x => x.Page);

            var pageItems = await context.PostalFeed.CountAsync(x => x.Page == maxPage);
            return pageItems >= maxPageSize ? maxPage + 1 : maxPage;
        }
    }
}
