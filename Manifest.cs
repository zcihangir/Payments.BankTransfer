using Grand.Infrastructure.Plugins;
using Payments.BankTransfer;

[assembly: PluginInfo(
    FriendlyName = "Bank transfer QR",
    Group = "Payment methods",
    SystemName = BankTransferPaymentDefaults.ProviderSystemName,
    Author = "Zafer Cihangir",
    Version = "2.20"
)]