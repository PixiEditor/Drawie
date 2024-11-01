// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import {dotnet} from './_framework/dotnet.js'

let canvasContextHandleIds = 0;
let canvasContextHandles = {};

let shaderHandleIds = 0;
let shaderHandles = {};

let programHandleIds = 0;
let programHandles = {};

let bufferHandleIds = 0;
let bufferHandles = {};

const {setModuleImports, getAssemblyExports, getConfig} = await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

setModuleImports('main.js', {
    interop: {
        invokeJs: (js) => eval(js),
    },
    canvas: {
        openContext: (canvasId, contextType) => {
            const canvas = document.getElementById(canvasId);
            console.assert(canvas !== null, `No canvas element found with id '${canvasId}'`);
            const context = canvas.getContext(contextType);
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
    webgl: {
        createShader: (glHandle, type) => {
            const gl = canvasContextHandles[glHandle];
            const shader = gl.createShader(type);
            const handleId = shaderHandleIds++;
            shaderHandles[handleId] = shader;
            
            return handleId;
        },
        shaderSource: (handleId, shaderId, source) => {
            const gl = canvasContextHandles[handleId];
            const shader = shaderHandles[shaderId];
            gl.shaderSource(shader, source);
        },
        compileShader: (handleId, shaderId) => {
            const gl = canvasContextHandles[handleId];
            const shader = shaderHandles[shaderId];
            gl.compileShader(shader);
            
            if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
                const info = gl.getShaderInfoLog(shader);
                gl.deleteShader(shader);
                return info;
            }
            
            return null;
        },
        createProgram: (glHandle) => {
            const gl = canvasContextHandles[glHandle];

            const program = gl.createProgram();
            programHandleIds++;
            programHandles[programHandleIds] = program;
            
            return programHandleIds;
        },
        attachShader: (glHandle, programId, shaderId) => {
            const gl = canvasContextHandles[glHandle];
            const program = programHandles[programId];
            const shader = shaderHandles[shaderId];
            gl.attachShader(program, shader);
        },
        linkProgram: (glHandle, programId) => {
            const gl = canvasContextHandles[glHandle];
            const program = programHandles[programId];
            gl.linkProgram(program);
            
            if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
                const info = gl.getProgramInfoLog(program);
                gl.deleteProgram(program);
                return info;
            }
            
            return null;
        },
        createBuffer: (glHandle) => {
            const gl = canvasContextHandles[glHandle];
            
            const buffer = gl.createBuffer();
            bufferHandleIds++;
            bufferHandles[bufferHandleIds] = buffer;
            
            return bufferHandleIds;
        },
        bindBuffer: (glHandle, target, bufferId) => {
            const gl = canvasContextHandles[glHandle];
            const buffer = bufferHandles[bufferId];
            gl.bindBuffer(target, buffer);
        },
        bufferData: (glHandle, target, data, usage) => {
            const gl = canvasContextHandles[glHandle];
            gl.bufferData(target, new Float32Array(data), usage);
        },
        clearColor: (glHandle, r, g, b, a) => {
            const gl = canvasContextHandles[glHandle];
            gl.clearColor(r, g, b, a);
        },
        clear: (glHandle, mask) => {
            const gl = canvasContextHandles[glHandle];
            gl.clear(mask);
        },
        vertexAttribPointer: (glHandle, index, size, type, normalized, stride, offset) => {
            const gl = canvasContextHandles[glHandle];
            gl.vertexAttribPointer(index, size, type, normalized, stride, offset);
        },
        enableVertexAttribArray: (glHandle, index) => {
            const gl = canvasContextHandles[glHandle];
            gl.enableVertexAttribArray(index);
        },
        useProgram: (glHandle, programId) => {
            const gl = canvasContextHandles[glHandle];
            const program = programHandles[programId];
            gl.useProgram(program);
        },
        drawArrays: (glHandle, mode, first, count) => {
            const gl = canvasContextHandles[glHandle];
            gl.drawArrays(mode, first, count);
        },
        getAttribLocation: (glHandle, programId, name) => {
            const gl = canvasContextHandles[glHandle];
            const program = programHandles[programId];
            return gl.getAttribLocation(program, name);
        },
        getProcAddress: (name) => {
            return WebGL2RenderingContext.prototype[name];
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