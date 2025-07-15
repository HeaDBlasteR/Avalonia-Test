using AvaloniaTests.Models;
using System;
using System.Collections.Generic;

namespace AvaloniaTests.Services
{
    public interface ITestService
    {
        List<Test> GetTests();
        void SaveTest(Test test);
        void DeleteTest(Guid testId);
    }
}
