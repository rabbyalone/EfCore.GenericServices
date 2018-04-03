﻿// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace GenericServices
{
    public interface IGenericServiceAsync<TContext> : IGenericServiceAsync where TContext : DbContext { }
}