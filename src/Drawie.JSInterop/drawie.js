export class Drawie {
    canvasContextHandles = {};

    shaderHandleIds = 0;
    shaderHandles = {};

    programHandleIds = 0;
    programHandles = {};

    bufferHandleIds = 0;
    bufferHandles = {};

    textureHandleIds = 0;
    textureHandles = {};

    uniformLocationHandleIds = 0;
    uniformLocationHandles = {};

    exports = {};

    addDrawieImports() {
        globalThis.getDotnetRuntime(0).setModuleImports('drawie.js', {
            interop: {
                invokeJs: (js) => eval(js),
            },
            webgl: {
                createShader: (glHandle, type) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const shader = gl.createShader(type);
                    const handleId = this.shaderHandleIds++;
                    this.shaderHandles[handleId] = shader;

                    return handleId;
                },
                shaderSource: (handleId, shaderId, source) => {
                    const gl = this.canvasContextHandles[handleId];
                    const shader = this.shaderHandles[shaderId];
                    gl.shaderSource(shader, source);
                },
                compileShader: (handleId, shaderId) => {
                    const gl = this.canvasContextHandles[handleId];
                    const shader = this.shaderHandles[shaderId];
                    gl.compileShader(shader);

                    if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
                        const info = gl.getShaderInfoLog(shader);
                        gl.deleteShader(shader);
                        return info;
                    }

                    return null;
                },
                createProgram: (glHandle) => {
                    const gl = this.canvasContextHandles[glHandle];

                    const program = gl.createProgram();
                    this.programHandleIds++;
                    this.programHandles[this.programHandleIds] = program;

                    return this.programHandleIds;
                },
                attachShader: (glHandle, programId, shaderId) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const program = this.programHandles[programId];
                    const shader = this.shaderHandles[shaderId];
                    gl.attachShader(program, shader);
                },
                linkProgram: (glHandle, programId) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const program = this.programHandles[programId];
                    gl.linkProgram(program);

                    if (!gl.getProgramParameter(program, gl.LINK_STATUS)) {
                        const info = gl.getProgramInfoLog(program);
                        gl.deleteProgram(program);
                        return info;
                    }

                    return null;
                },
                createBuffer: (glHandle) => {
                    const gl = this.canvasContextHandles[glHandle];

                    const buffer = gl.createBuffer();
                    this.bufferHandleIds++;
                    this.bufferHandles[this.bufferHandleIds] = buffer;

                    return this.bufferHandleIds;
                },
                bindBuffer: (glHandle, target, bufferId) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const buffer = this.bufferHandles[bufferId];
                    gl.bindBuffer(target, buffer);
                },
                bufferData: (glHandle, target, data, usage) => {
                    const gl = this.canvasContextHandles[glHandle];
                    gl.bufferData(target, new Float32Array(data), usage);
                },
                clearColor: (glHandle, r, g, b, a) => {
                    const gl = this.canvasContextHandles[glHandle];
                    gl.clearColor(r, g, b, a);
                },
                clear: (glHandle, mask) => {
                    const gl = this.canvasContextHandles[glHandle];
                    gl.clear(mask);
                },
                vertexAttribPointer: (glHandle, index, size, type, normalized, stride, offset) => {
                    const gl = this.canvasContextHandles[glHandle];
                    gl.vertexAttribPointer(index, size, type, normalized, stride, offset);
                },
                enableVertexAttribArray: (glHandle, index) => {
                    const gl = this.canvasContextHandles[glHandle];
                    gl.enableVertexAttribArray(index);
                },
                useProgram: (glHandle, programId) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const program = programHandles[programId];
                    gl.useProgram(program);
                },
                drawArrays: (glHandle, mode, first, count) => {
                    const gl = this.canvasContextHandles[glHandle];
                    gl.drawArrays(mode, first, count);
                },
                getAttribLocation: (glHandle, programId, name) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const program = this.programHandles[programId];
                    return gl.getAttribLocation(program, name);
                },
                openSkiaContext: (canvasId) => {
                    const contextAttributes = {
                        alpha: 1,
                        depth: 1,
                        stencil: 8,
                        antialias: 1,
                        premultipliedAlpha: 1,
                        preserveDrawingBuffer: 0,
                        preferLowPowerToHighPerformance: 0,
                        failIfMajorPerformanceCaveat: 0,
                        majorVersion: 2,
                        minorVersion: 0,
                        enableExtensionsByDefault: 1,
                        explicitSwapControl: 0,
                        renderViaOffscreenBackBuffer: 0,
                    };

                    const canvas = document.getElementById(canvasId);
                    const handle = globalThis.SkiaSharpGL.createContext(canvas, contextAttributes);
                    this.canvasContextHandles[handle] = globalThis.SkiaSharpGL.getContext(handle).GLctx;
                    return handle;
                },
                makeContextCurrent: (handle) => {
                    globalThis.SkiaSharpGL.makeContextCurrent(handle);
                },
                createTexture: (glHandle) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const texture = gl.createTexture();
                    this.textureHandleIds++;
                    this.textureHandles[this.textureHandleIds] = texture;
                    return this.textureHandleIds;
                },
                bindTexture: (glHandle, target, textureId) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const texture = this.textureHandles[textureId];
                    gl.bindTexture(target, texture);
                },
                texImage2D: (glHandle, target, level, internalformat, width, height, border, format, type, offset) => {
                    const gl = this.canvasContextHandles[glHandle];
                    gl.texImage2D(target, level, internalformat, width, height, border, format, type, null);
                },
                texParameteri: (glHandle, target, pname, param) => {
                    const gl = this.canvasContextHandles[glHandle];
                    gl.texParameteri(target, pname, param);
                },
                activeTexture: (glHandle, textureUnit) => {
                    const gl = this.canvasContextHandles[glHandle];
                    gl.activeTexture(gl.TEXTURE0 + textureUnit);
                },
                uniform1i: (glHandle, location, value) => {
                    const gl = this.canvasContextHandles[glHandle];

                    const uniformLocation = this.uniformLocationHandles[location];
                    gl.uniform1i(uniformLocation, value);
                },
                getUniformLocation: (glHandle, programId, name) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const program = this.programHandles[programId];
                    const location = gl.getUniformLocation(program, name);

                    this.uniformLocationHandleIds++;
                    this.uniformLocationHandles[this.uniformLocationHandleIds] = location;
                    return this.uniformLocationHandleIds;
                },
                deleteTexture: (glHandle, textureId) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const texture = this.textureHandles[textureId];
                    gl.deleteTexture(texture);

                    delete this.textureHandles[textureId];
                },
            },
            window: {
                innerWidth: () => window.innerWidth,
                innerHeight: () => window.innerHeight,
                requestAnimationFrame: () => this.invokeRequestAnimationFrame(),
                subscribeWindowResize: () => window.addEventListener('resize', this.invokeWindowResize)
            },
            input: {
                subscribeKeyDown: () => {
                    document.addEventListener('keydown', (event) => {
                        this.exports.Drawie.JSInterop.JSRuntime.OnKeyDown(event.key);
                    });
                },
                subscribeKeyUp: () => {
                    document.addEventListener('keyup', (event) => {
                        this.exports.Drawie.JSInterop.JSRuntime.OnKeyUp(event.key);
                    });
                },
            }
        });
    }

    invokeRequestAnimationFrame() {
        const startTime = performance.now();
        const requestId = requestAnimationFrame(() => {
            const endTime = performance.now();
            const dt = endTime - startTime;
            this.exports.Drawie.JSInterop.JSRuntime.OnAnimationFrame(dt);
        });

        return requestId;
    }

    invokeWindowResize() {
        if (this.exports) {
            this.exports.Drawie.JSInterop.JSRuntime.WindowResized(window.innerWidth, window.innerHeight);
        }
    }

    async addDrawieExports() {
        this.exports = await globalThis.getDotnetRuntime(0).getAssemblyExports("Drawie.JSInterop");
    }
}