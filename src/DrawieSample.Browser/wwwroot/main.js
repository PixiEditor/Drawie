// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import { dotnet } from './_framework/dotnet.js'

const { setModuleImports, getAssemblyExports, getConfig } = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

setModuleImports('main.js', {
    interop: {
        invokeJs: (js) => eval(js)
    },
    window: {
        innerWidth: () => window.innerWidth,
        innerHeight: () => window.innerHeight
    }
});

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

await dotnet.run();