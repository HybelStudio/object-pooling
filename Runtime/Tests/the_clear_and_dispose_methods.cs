using System.Linq;
using NUnit.Framework;
using FluentNUnity.Shared;

namespace Hybel.ObjectPooling.Tests
{
    public partial class object_pool_tests
    {
        public class the_clear_and_dispose_methods
        {
            [Test]
            public void clear_empties_the_pool_full_of_inactive_objects()
            {
                var pool = FullObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                pool.CountAll.Should().Be(5);
                pool.CountActive.Should().Be(0);
                pool.CountInactive.Should().Be(5);
                pool.Clear();
                pool.CountAll.Should().Be(0);
                pool.CountActive.Should().Be(0);
                pool.CountInactive.Should().Be(0);
            }

            [Test]
            public void clear_empties_the_pool_full_of_active_objects()
            {
                var pool = FullObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                pool.Get(MAX_POOL_SIZE).ToList(); // Get all items then to list to enumerate. This activates all the items.
                pool.CountAll.Should().Be(5);
                pool.CountActive.Should().Be(5);
                pool.CountInactive.Should().Be(0);
                pool.Clear();
                pool.CountAll.Should().Be(0);
                pool.CountActive.Should().Be(0);
                pool.CountInactive.Should().Be(0);
            }

            [Test]
            public void clear_calls_on_destroy_method()
            {
                var pool = FullObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                var obj = pool.Get();
                obj.Destroyed.Should().BeFalse();
                pool.Clear();
                obj.Destroyed.Should().BeTrue();
            }
        }
    }
}