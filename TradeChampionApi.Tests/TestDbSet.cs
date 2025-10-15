using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Helper for creating a mock DbSet<T> from a list for unit tests
/// </summary>

public static class TestDbSet
{
    public static DbSet<T> Create<T>(IEnumerable<T> data) where T : class
    {
        var queryable = data.AsQueryable();

        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

        // Add support for Add()
        mockSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>(item =>
        {
            if (data is IList<T> list)
                list.Add(item);
        });

        mockSet.Setup(d => d.AddRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>(items =>
        {
            if (data is IList<T> list)
                foreach (var item in items)
                    list.Add(item);
        });

        mockSet.Setup(d => d.Remove(It.IsAny<T>())).Callback<T>(item =>
        {
            if (data is IList<T> list)
                list.Remove(item);
        });

        mockSet.Setup(d => d.RemoveRange(It.IsAny<IEnumerable<T>>())).Callback<IEnumerable<T>>(items =>
        {
            if (data is IList<T> list)
                foreach (var item in items)
                    list.Remove(item);
        });

        return mockSet.Object;
    }
}
