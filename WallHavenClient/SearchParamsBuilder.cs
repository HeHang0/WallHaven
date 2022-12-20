using System.Collections.Generic;
using System.Text;

namespace WallHaven.WallHavenClient
{
    public enum Sorting
    {
        date_added = 1,
        relevance = 2,
        random = 3,
        views = 4,
        favourites = 5,
        toplist = 6,
        hot = 7
    }

    public enum OrderBy
    {
        desc = 1,
        asc = 2
    }

    public enum TopRange
    {
        _1d = 1,
        _3d = 2,
        _1w = 3,
        _1M = 4,
        _3M = 5,
        _6M = 6,
        _1y = 7
    }

    public static class Ratio
    {
        public static string Wide = "16x9,16x10";
        public static string UltraWide = "21x9,32x10,48x10";
        public static string Portrait = "9x16,10x16,9x18";
        public static string Square = "1x1,3x2,4x3,5x4";
    }

    public class SearchParamsBuilder
    {
        private string _apiKey = string.Empty;
        private bool _searchGeneral;
        private bool _searchAnime;
        private bool _searchPeople;
        private bool _searchSFW;
        private bool _searchSketchy;
        private bool _searchNSFW;
        private Sorting _sorting;
        private OrderBy _orderBy;
        private TopRange _topRange;
        private int _minWidth;
        private int _minHeight;
        private List<string> _resolutions = new List<string>();
        private List<string> _ratios = new List<string>();

        public string Build()
        {
            List<string> builder = new List<string>();

            if (!string.IsNullOrEmpty(_apiKey))
            {
                builder.Add($"apikey={_apiKey}");
            }

            string categories = string.Empty;
            categories += _searchGeneral ? "1" : "0";
            categories += _searchAnime ? "1" : "0";
            categories += _searchPeople ? "1" : "0";
            builder.Add($"categories={categories}");

            string purity = string.Empty;
            purity += _searchSFW ? "1" : "0";
            purity += _searchSketchy ? "1" : "0";
            purity += _searchNSFW ? "1" : "0";
            builder.Add($"purity={purity}");

            builder.Add($"sorting={_sorting}");
            builder.Add($"order={_orderBy}");

            if (_sorting is Sorting.toplist)
            {
                builder.Add($"topRange={_topRange.ToString().Replace("_", "")}");
            }

            if (_minHeight != 0 && _minWidth != 0)
            {
                builder.Add($"atleast={_minWidth}x{_minHeight}");
            }

            if (_resolutions.Count > 0)
            {
                builder.Add($"resolutions={string.Join(",", _resolutions)}");
            }

            if (_ratios.Count > 0)
            {
                builder.Add($"ratios={string.Join(",", _ratios)}");
            }
            
            var finalQueryString = "?"+string.Join("&", builder); ;
            return finalQueryString;
        }

        public SearchParamsBuilder WithApiKey(string apiKey)
        {
            _apiKey = apiKey;
            return this;
        }

        public SearchParamsBuilder WithMinimumResolution(int width, int height)
        {
            _minHeight = height;
            _minWidth = width;
            return this;
        }

        public SearchParamsBuilder IncludeGeneral(bool includeGeneral)
        {
            _searchGeneral = includeGeneral;
            return this;
        }

        public SearchParamsBuilder IncludeAnime(bool includeAnime)
        {
            _searchAnime = includeAnime;
            return this;
        }

        public SearchParamsBuilder IncludePeople(bool includePeople)
        {
            _searchPeople = includePeople;
            return this;
        }

        public SearchParamsBuilder IncludeSafe(bool includeSafe)
        {
            _searchSFW = includeSafe;
            return this;
        }

        public SearchParamsBuilder IncludeSketchy(bool includeSketchy)
        {
            _searchSketchy = includeSketchy;
            return this;
        }

        public SearchParamsBuilder IncludeNSFW(bool includeNSFW)
        {
            _searchNSFW = includeNSFW;
            return this;
        }

        public SearchParamsBuilder SortBy(Sorting sorting)
        {
            _sorting = sorting;
            return this;
        }

        public SearchParamsBuilder OrderBy(OrderBy orderBy)
        {
            _orderBy = orderBy;
            return this;
        }

        public SearchParamsBuilder SelectTop(TopRange topRange)
        {
            _topRange = topRange;
            return this;
        }

        public SearchParamsBuilder WithResolutions(List<string> resolutions)
        {
            _resolutions = resolutions;
            return this;
        }

        public SearchParamsBuilder WithRatios(List<string> ratios)
        {
            _ratios = ratios;
            return this;
        }
    }
}