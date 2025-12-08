using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using LayeredMicroservice.Domain;
using LayeredMicroservice.Infrastructure;
using LayeredMicroservice.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LayeredMicroservice.Tests;

public sealed class SqlOrderRepositoryTests
{
    private readonly Mock<DbConnection> _connection = new();
    private readonly Mock<DbCommand> _command = new();
    private readonly Mock<ILogger<SqlOrderRepository>> _logger = new();

    [SetUp]
    public void SetUp()
    {
        _connection.Setup(c => c.CreateCommand()).Returns(_command.Object);
        _command.SetupGet(c => c.Parameters).Returns(new FakeParameterCollection());
    }

    [Test]
    public async Task SaveAsync_WhenConstraintViolation_ReturnsDomainError()
    {
        _command.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TestDbException("constraint"));

        var repo = new SqlOrderRepository(_connection.Object, _logger.Object);
        var order = Order.Create(Guid.NewGuid(), new[] { new OrderLineDraft("SKU", 1) }, DateTimeOffset.UtcNow);

        var result = await repo.SaveAsync(order, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Code, Is.EqualTo(ErrorCode.Domain));
    }

    [Test]
    public async Task SaveAsync_WhenUnexpectedException_ReturnsUnexpected()
    {
        _command.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TestDbException("timeout"));

        var repo = new SqlOrderRepository(_connection.Object, _logger.Object);
        var order = Order.Create(Guid.NewGuid(), new[] { new OrderLineDraft("SKU", 1) }, DateTimeOffset.UtcNow);

        var result = await repo.SaveAsync(order, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Code, Is.EqualTo(ErrorCode.Unexpected));
    }

    private sealed class FakeParameterCollection : DbParameterCollection
    {
        private readonly List<object> _items = new();

        public override int Add(object value)
        {
            _items.Add(value);
            return _items.Count - 1;
        }

        public override bool Contains(object value) => _items.Contains(value);
        public override void Clear() => _items.Clear();
        public override int IndexOf(object value) => _items.IndexOf(value);
        public override void Insert(int index, object value) => _items.Insert(index, value);
        public override void Remove(object value) => _items.Remove(value);
        public override void RemoveAt(int index) => _items.RemoveAt(index);
        public override object this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }

        public override bool Contains(string value) => throw new NotSupportedException();
        public override int IndexOf(string parameterName) => throw new NotSupportedException();
        public override void RemoveAt(string parameterName) => throw new NotSupportedException();
        protected override DbParameter GetParameter(int index) => throw new NotSupportedException();
        protected override DbParameter GetParameter(string parameterName) => throw new NotSupportedException();
        public override void AddRange(Array values) => throw new NotSupportedException();
        public override int Count => _items.Count;
        public override object SyncRoot => this;
        public override bool IsSynchronized => false;
        public override bool IsFixedSize => false;
        public override bool IsReadOnly => false;
        public override IEnumerator GetEnumerator() => _items.GetEnumerator();
        protected override void SetParameter(int index, DbParameter value) => throw new NotSupportedException();
        protected override void SetParameter(string parameterName, DbParameter value) => throw new NotSupportedException();
    }

    private sealed class TestDbException : DbException
    {
        public TestDbException(string message) : base(message)
        {
        }
    }
}
