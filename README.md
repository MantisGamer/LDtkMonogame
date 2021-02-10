<p align="center">
 <a href="https://github.com/deepnight/ldtk"> <img alt="LDtk Version Support" src="https://img.shields.io/github/v/release/deepnight/ldtk?&label=Supports%20LDtk"></a>
<a href="https://www.nuget.org/packages/LDtkMonogame/"><img src="https://img.shields.io/nuget/v/LDtkMonogame?" /></a>
<a href="https://www.nuget.org/packages/LDtkMonogame/"><img alt="Nuget" src="https://img.shields.io/nuget/dt/LDtkMonogame"></a>
</p>
<p align="center">
  <img alt="GitHub Workflow Status" src="https://img.shields.io/github/workflow/status/IrishBruse/LDtkMonogame/Build">
</p>

# LDtk Monogame
LDtk Monogame is a [LDtk](https://ldtk.io) project loader and renderer for the [Monogame](https://www.monogame.net/) Framework


![LDtk to Monogame Conversion](art/readme/LDtk%20to%20Monogame.png "1 to 1 Conversion")
 
# Quick Start Guide
The easiest way to start using LDtkMonogame is to import it into the project using nuget  

<a href="https://www.nuget.org/packages/LDtkMonogame/"><img src="https://img.shields.io/nuget/v/LDtkMonogame?" /></a>

Make sure to import the namespace at the top
```csharp
using LDtk;
``` 

To get started loading ldtk files create a  
```csharp
World ldtkWorld = new World("Assets/MyProject.ldtk");
``` 
 
Now just load the level
```csharp
Level startLevel = ldtkWorld.GetLevel("Level1");
```  

Now to render the level we loaded in `Draw`
```csharp
spriteBatch.Begin(SpriteSortMode.Texture, samplerState: SamplerState.PointClamp);
{
    for(int i = 0; i < startLevel.Layers.Length; i++)
    {
        spriteBatch.Draw(startLevel.Layers[i], startLevel.WorldPosition, Color.White);
    }
}
spriteBatch.End();
```
Thats all thats needed to render your level everything else is handled by LDtkMonogame
  
### For a more detailed introduction and on how to use `IntGrids` and `Entities` check out the wiki

[Wiki and Api Documentation](https://irishbruse.github.io/LDtkMonogame/)
------------

# Example

This small game example [LDtkMonogame.Examples](https://github.com/IrishBruse/LDtkMonogame/tree/main/LDtkMonogame.Examples) showcases how easy it is to get setup and making levels for your game

## How to run
- Open the solution and hit run in visual studio or
- `cd` into the `LDtkMonogame.Examples` folder and use `dotnet run` to play the example game

You can even edit the .ldtk file and run it again to see the changes

![Example Gameplay](art/readme/Example%20Project.gif "Gameplay")