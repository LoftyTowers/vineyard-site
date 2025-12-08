public interface IPaymentGateway { Task<bool> PayAsync(Guid orderId, decimal amount, CancellationToken ct); }

public sealed class StripeGateway : IPaymentGateway { public Task<bool> PayAsync(Guid id, decimal amount, CancellationToken ct) => Task.FromResult(true); }
public sealed class SandboxGateway : IPaymentGateway { public Task<bool> PayAsync(Guid id, decimal amount, CancellationToken ct) => Task.FromResult(true); }

public interface IPaymentGatewayFactory { IPaymentGateway Create(); }

public sealed class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IConfiguration _cfg; private readonly IServiceProvider _sp;
    public PaymentGatewayFactory(IConfiguration cfg, IServiceProvider sp) { _cfg = cfg; _sp = sp; }

    public IPaymentGateway Create()
    {
        var sandbox = _cfg.GetValue<bool>("Payments:Sandbox");
        return sandbox ? _sp.GetRequiredService<SandboxGateway>()
                       : _sp.GetRequiredService<StripeGateway>();
    }
}
