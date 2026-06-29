global using FluentValidation;
global using MediatR;
global using Microsoft.AspNetCore.Diagnostics;
global using Microsoft.AspNetCore.Mvc;
global using WexTransaction.Api;
global using WexTransaction.Api.Exceptions;
global using WexTransaction.Api.Extensions;
global using WexTransaction.Application.UseCases.GetPurchaseTransaction;
global using WexTransaction.Application.UseCases.SavePurchaseTransaction;
global using WexTransaction.CrossCutting.AppDependencies;
global using WexTransaction.Domain.Exceptions;
global using Serilog;
global using Serilog.Events;
global using HealthChecks.UI.Client;
global using Microsoft.AspNetCore.Diagnostics.HealthChecks;

