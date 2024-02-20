﻿using Grand.Business.Core.Interfaces.Checkout.Payments;
using Grand.Business.Core.Interfaces.Cms;
using Grand.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.BankTransfer.Infrastructure;

namespace Payments.BankTransfer
{
    public class StartupApplication : IStartupApplication
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IBankTransferService, BankTransferService>();
            services.AddScoped<IBankTransferMessageProvider, BankTransferMessageProvider>();
            services.AddScoped<IPaymentProvider, BankTransferPaymentProvider>();
            services.AddScoped<IWidgetProvider, BankTransferPaymentWidgetProvider>();
        }

        public int Priority => 10;

        public void Configure(IApplicationBuilder application, IWebHostEnvironment webHostEnvironment)
        {

        }

        public bool BeforeConfigure => false;
    }

}
