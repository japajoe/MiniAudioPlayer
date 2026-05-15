namespace MiniAudioPlayer.Embedded
{
    public static class BasicShader
    {
        private static readonly string vertexSource = @"#version 330 core
out vec2 TexCoords;

void main() {
    // Generates a triangle that covers the [-1, 1] range
    // Vertex 0: (-1, -1), UV (0, 0)
    // Vertex 1: ( 3, -1), UV (2, 0)
    // Vertex 2: (-1,  3), UV (0, 2)
    
    float x = -1.0 + float((gl_VertexID & 1) << 2);
    float y = -1.0 + float((gl_VertexID & 2) << 1);
    
    TexCoords.x = (x + 1.0) * 0.5;
    TexCoords.y = (y + 1.0) * 0.5;
    
    gl_Position = vec4(x, y, 0.0, 1.0);
}";
        private static readonly string fragmentSource = @"void main() {
    // 1. Sample the frequency magnitude from the green channel
    // Using TexCoords.x to map the 512 bins across the screen width
    float magnitude = texture(uAudio, vec2(TexCoords.x * 0.5, 0.5)).g;

    // 2. Adjust scaling
    // Magnitude from FFT can be small; boost it to fill vertical space
    float barHeight = magnitude * 2.5;

    // 3. Create the bar 
    // step() returns 1.0 if the vertical pixel is below the barHeight
    float bar = step(TexCoords.y, barHeight * 3.0);

    // 4. Color Logic
    // Create a vertical gradient: Blue at bottom, Green at top
    vec3 barColor = mix(vec3(0.0, 0.5, 1.0), vec3(0.0, 1.0, 0.2), TexCoords.y);
    vec3 currentFrame = barColor * bar;

    // 5. Feedback Loop (Ghosting/Trails)
    // Sample previous frame with a slight downward 'melt'
    vec2 trailUV = TexCoords + vec2(0.0, 0.005); 
    vec3 prevFrame = texture(uTexture, trailUV).rgb;

    // Use max to keep the bright live bars while fading the old ones
    float decay = 0.85;
    //vec3 finalColor = max(currentFrame, prevFrame * decay);
    vec3 finalColor = currentFrame;

    FragColor = vec4(finalColor, 1.0);
}

void main2() {
    // 1. Audio sampling and centering
    float raw = texture(uAudio, vec2(TexCoords.x, 0.5)).r;
    float waveY = raw * 0.5 + 0.5;
    
    // 2. Waveform drawing
    // Narrow thickness prevents the whole screen from turning green
    float dist = abs(TexCoords.y - waveY);
    float currentLine = smoothstep(0.004, 0.002, dist);
    vec3 currentColor = vec3(0.55, 0.89, 0.40) * currentLine;

    // 3. Feedback logic
    // Sample with a slight zoom (99.5%) to create movement
    vec2 zoomUV = (TexCoords - 0.5) * 0.995 + 0.5;
    vec3 prevFrame = texture(uTexture, zoomUV).rgb;

    // 4. Persistence and combine
    // 0.88 decay ensures trails fade out within a few frames
    // max() prevents the additive white-out/green-out
    float decay = 0.88;
    vec3 finalRGB = max(currentColor, prevFrame * decay);

    // 5. Final output
    // Forced 1.0 alpha to prevent transparency issues in the buffer swap
    FragColor = vec4(finalRGB, 1.0);
}";

        private static readonly string headerSource = @"#version 330 core

uniform sampler2D uTexture;
uniform sampler2D uAudio;
uniform float uTime;
uniform vec2 uResolution;

in vec2 TexCoords;
out vec4 FragColor;";

        public static string VertexSource => vertexSource;
        public static string FragmentSource => fragmentSource;
        public static string HeaderSource => headerSource;
    }
}