using FluentNUnity.Shared;
using NUnit.Framework;

namespace Hybel.ObjectPooling.Tests
{
    public partial class object_pool_tests
    {
        public class the_release_method
        {
            [Test]
            public void release_increases_inactive_count_and_decreases_active_count()
            {
                var pool = EmptyObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                var obj = pool.Get();
                pool.CountInactive.Should().Be(0);
                pool.CountActive.Should().Be(1);
                pool.Release(obj);
                pool.CountInactive.Should().Be(1);
                pool.CountActive.Should().Be(0);
            }

            [Test]
            public void release_calls_on_release_method()
            {
                var pool = EmptyObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                var obj = pool.Get();
                pool.Release(obj);
                obj.Enabled.Should().BeFalse();
            }

            [Test]
            public void release_does_nothing_if_pool_is_empty_and_is_passed_null()
            {
                var pool = EmptyObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                pool.Release(null);
                pool.CountInactive.Should().Be(0);
            }

            [Test]
            public void release_does_nothing_when_releasing_foreign_obj()
            {
                var pool = EmptyObjectPoolWithDestroyFuncAndMaxSizeOfFive;
                pool.Release(new PooledObject { Destroyed = false });
                pool.CountAll.Should().Be(0);
                pool.CountActive.Should().Be(0);
                pool.CountInactive.Should().Be(0);
            }
        }
    }
}