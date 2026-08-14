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

    samplerIds = 0;
    samplerHandles = {}

    framebufferIds = 0;
    framebufferHandles = {}

    uniformLocationHandleIds = 0;
    uniformLocationHandles = {};

    vertexArrayIds = 0;
    vertexArrayHandles = {}
    
    renderbufferIds = 0;
    renderbufferHandles = {}

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
                viewport: (handleId, x, y, width, height) => {
                    const gl = this.canvasContextHandles[handleId];
                    gl.viewport(x, y, width, height);
                },
                createFramebuffer: (glHandle) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const webGlFramebuffer = gl.createFramebuffer();
                    this.framebufferIds++;
                    this.framebufferHandles[this.framebufferIds] = webGlFramebuffer;
                    return this.framebufferIds;
                },
                bindFramebuffer: (handleId, target, framebuffer) => {
                    const gl = this.canvasContextHandles[handleId];
                    const fb = this.framebufferHandles[framebuffer];
                    gl.bindFramebuffer(target, fb);
                },
                framebufferTexture2D: (glHandle, target, attachment, textarget, texture, level) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const targetTexture = this.textureHandles[texture]
                    gl.framebufferTexture2D(target, attachment, textarget, targetTexture, level);
                },
                checkFramebufferStatus: (glHandle, target) => {
                    const gl = this.canvasContextHandles[glHandle];
                    return gl.checkFramebufferStatus(target);
                },
                getError: (glHandle) => {
                    const gl = this.canvasContextHandles[glHandle];
                    return gl.getError();
                },
                deleteFramebuffer: (glHandle, framebuffer) => {
                    const gl = this.canvasContextHandles[glHandle];
                    gl.deleteFramebuffer(this.framebufferHandles[framebuffer]);
                    delete this.framebufferHandles[framebuffer];
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
                bufferData: (glHandle, target, dataOrSize, usage) => {
                    const gl = this.canvasContextHandles[glHandle];
                    if (typeof dataOrSize === 'number') {
                        gl.bufferData(target, dataOrSize, usage);
                        return;
                    }

                    const array = target === 0x8893 ? new Uint16Array(dataOrSize) : new Float32Array(dataOrSize);
                    gl.bufferData(target, array, usage);
                },
                bindBufferBase: (glHandle, target, index, buffer) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const bufferObj = this.bufferHandles[buffer];
                    gl.bindBufferBase(target, index, bufferObj);
                },
                bufferSubData: (glHandle, target, dstByteOffset, srcData) => {
                    const gl = this.canvasContextHandles[glHandle];

                    const data = srcData instanceof Uint8Array
                        ? srcData
                        : new Uint8Array(srcData);

                    gl.bufferSubData(
                        target,
                        dstByteOffset,
                        data
                    );
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
                    const program = this.programHandles[programId];
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
                enable: (glHandle, cap) => {
                    const gl = this.canvasContextHandles[glHandle];
                    gl.enable(cap);
                },
                disable: (glHandle, cap) => {
                    const gl = this.canvasContextHandles[glHandle];
                    gl.disable(cap)
                },
                depthFunc: (glHandle, func) => {
                    const gl = this.canvasContextHandles[glHandle];
                    gl.depthFunc(func);
                },
                clearDepth: (glHandle, depth) => {
                    const gl = this.canvasContextHandles[glHandle];
                    gl.clearDepth(depth);
                },
                depthMask: (glHandle, value) => {
                    const gl = this.canvasContextHandles[glHandle];
                    gl.depthMask(value);
                },
                getParameter: (glHandle, param) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const foundParam = gl.getParameter(param);
                    return foundParam.name;
                },
                bindVertexArray: (glHandle, vertexArray) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const vao = this.vertexArrayHandles[vertexArray];
                    gl.bindVertexArray(vao);
                },
                bindSampler: (glHandle, slot, sampler) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const wglSampler = this.samplerHandles[sampler];
                    gl.bindSampler(slot, wglSampler);
                },
                uniformBlockBinding: (glHandle, program, blockIndex, bindingPoint) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const wglProgram = this.programHandles[program];
                    gl.uniformBlockBinding(wglProgram, blockIndex, bindingPoint);
                },
                createRenderbuffer: (glHandle) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const rb = gl.createRenderbuffer()
                    this.renderbufferIds++;
                    this.renderbufferHandles[this.renderbufferIds] = rb;
                    return this.renderbufferIds;
                },
                bindRenderbuffer: (glHandle, target, renderbufferId) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const rb = this.renderbufferHandles[renderbufferId];
                    gl.bindRenderbuffer(target, rb);
                },
                renderbufferStorage: (glHandle, target, internalFormat, width, height) => {
                    const gl = this.canvasContextHandles[glHandle];
                    gl.renderbufferStorage(target, internalFormat, width, height);
                },
                deleteRenderbuffer: (glHandle, renderbufferId) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const rb = this.renderbufferHandles[renderbufferId];
                    gl.deleteRenderbuffer(rb);
                    delete this.renderbufferHandles[renderbufferId];
                },
                framebufferRenderbuffer: (glHandle, target, attachment, renderbufferTarget, renderbuffer) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const rb = this.renderbufferHandles[renderbuffer];
                    gl.framebufferRenderbuffer(target, attachment, renderbufferTarget, rb);
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
                    gl.activeTexture(textureUnit);
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
                drawElements: (glHandle, mode, count, type, offset) => {
                    const gl = this.canvasContextHandles[glHandle];
                    gl.drawElements(mode, count, type, offset);
                },
                blitFramebuffer: (glHandle, srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, mask, filter) => {
                    const gl = this.canvasContextHandles[glHandle];
                    gl.blitFramebuffer(srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, mask, filter);
                },
                createSampler: (glHandle) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const sampler = gl.createSampler();
                    this.samplerIds++;
                    this.samplerHandles[this.samplerIds] = sampler;
                    return this.samplerIds;
                },
                createVertexArray: (glHandle) => {
                    const gl = this.canvasContextHandles[glHandle];
                    const vao = gl.createVertexArray();
                    this.vertexArrayIds++;
                    this.vertexArrayHandles[this.vertexArrayIds] = vao;
                    return this.vertexArrayIds;
                }
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