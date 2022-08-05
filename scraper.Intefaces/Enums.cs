

namespace scraper.Core
{
    public enum ScrapTaskStage {Setup, Delaying, Ready, DownloadingData, Paused, ConvertingData, Success, Failed }


    public enum FieldRole
    {
        /// <summary>
        /// default, does nothing special
        /// </summary>
        none = 0,
        /// <summary>
        /// the field values must be string providing image location (either local files path or url's) which are then used to render the elements thumbnails (on List view mode at least)
        /// </summary>
        Thumbnail,
        /// <summary>
        /// drops the need for generating guids using this instead
        /// </summary>
        UID,
        /// <summary>
        /// the main title shown in ListView cards
        /// </summary>
        Title,
        /// <summary>
        /// shown under the main title in ListView cards
        /// </summary>
        SubTitle,
        /// <summary>
        /// fields such as product's category, or music genre, allow UI features such as grouping and used in stats.
        /// </summary>
        GroupBy
    }

   
}
