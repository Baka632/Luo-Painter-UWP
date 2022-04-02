## 像素着色器 

此 Shaders 文件夹包含供 PixelShaderEffect 使用的自定义像素着色器。


<br/>

## 开发环境

|Key|Value|
|:-|:-|
|开发工具|Visual Studio 2017|
|编程语言|HLSL|
|编译工具|开发者命令提示|
|编译命令|CompileShaders.cmd|
|着色器源代码|*.hlsl 文件|
|二进制文件|*.bin 文件|


<br/>

### 示例

```csharp
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Effects;
using Windows.Storage;
using Windows.Storage.Streams;
...
public static async Task<PixelShaderEffect> CreaAsync(IGraphicsEffectSource source, float parameter1)
{
    // TODO: 请把 "MyHLSL.bin" 替换为文件名。
    Uri uri = new Uri("ms-appx:///Shaders/MyHLSL.bin");

    // 读取所有字节。
    StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
    IBuffer buffer = await FileIO.ReadBufferAsync(file);
    byte[] shaderCode = buffer.ToArray();

    // 创建像素着色器。
    PixelShaderEffect shader = new PixelShaderEffect(shaderCode)
    {
        Source1BorderMode = EffectBorderMode.Hard,
        Source1 = source,
        Properties =
        {
            // TODO: 请使用 HLSL 参数替换 "Parameter1"。
            ["Parameter1"] = parameter1,
        }
    };

    return shader;
}
...
```

## 部署说明

> 启动 VS2017 的 开发人员 命令提示符，运行 CompileShaders.cmd。
> 
> 这将重新编译 *.hlsl 文件，生成 *.bin 输出二进制文件。
>
> 在解决方案视图中，将 *.bin 文件放入 "Shader" 文件夹；
>
> 在属性视图中，将 *.bin 文件的 ”生成操作“ 设置为 “内容”。