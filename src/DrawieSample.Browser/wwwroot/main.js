// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import {dotnet} from './_framework/dotnet.js'
import {Drawie} from "./scripts/drawie.js";

const {setModuleImports, getAssemblyExports, getConfig} = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

const drawie = new Drawie();

drawie.addDrawieImports();
await drawie.addDrawieExports();

const config = getConfig();
await dotnet.run();