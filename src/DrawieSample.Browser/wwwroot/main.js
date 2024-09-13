// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import {dotnet} from './_framework/dotnet.js'

let canvasContextHandleIds = 0;
let canvasContextHandles = {};

const {setModuleImports, getAssemblyExports, getConfig} = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

setModuleImports('main.js', {
    interop: {
        invokeJs: (js) => eval(js),
    },
    canvas: {
        openContext: (canvasId) => {
            const canvas = document.getElementById(canvasId);
            console.assert(canvas !== null, `No canvas element found with id '${canvasId}'`);
            const context = canvas.getContext('2d');
            const handleId = canvasContextHandleIds++;
            canvasContextHandles[handleId] = context;

            return handleId;
        },
        clearRect: (handleId, x, y, width, height) => {
            const context = canvasContextHandles[handleId];
            context.clearRect(x, y, width, height);
        },
        fillRect: (handleId, x, y, width, height) => {
            const context = canvasContextHandles[handleId];
            context.fillRect(x, y, width, height);
        },
        strokeRect: (handleId, x, y, width, height) => {
            const context = canvasContextHandles[handleId];
            context.strokeRect(x, y, width, height);
        },
        setFillStyle: (handleId, color) => {
            const context = canvasContextHandles[handleId];
            context.fillStyle = color;
        },
        setStrokeStyle: (handleId, color) => {
            const context = canvasContextHandles[handleId];
            context.strokeStyle = color;
        },
    },
    window: {
        innerWidth: () => window.innerWidth,
        innerHeight: () => window.innerHeight,
        requestAnimationFrame: () => invokeRequestAnimationFrame(),
    }
});

function invokeRequestAnimationFrame() {
    const startTime = performance.now();
    const requestId = requestAnimationFrame(() => {
        const endTime = performance.now();
        const dt = endTime - startTime;
        exports.Drawie.JSInterop.JSRuntime.OnAnimationFrame(dt);
    });

    return requestId;
}

const config = getConfig();
const exports = await getAssemblyExports("Drawie.JSInterop");

await dotnet.run();