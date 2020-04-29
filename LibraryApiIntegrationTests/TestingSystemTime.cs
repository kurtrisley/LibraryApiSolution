using LibraryApi.Services;
using System;

namespace LibraryApiIntegrationTests
{
    public class TestingSystemTime : ISystemTime
    {
        public DateTime GetCurrent()
        {
            return new DateTime(1969, 4, 20, 23, 59, 59);
        }
    }
}
