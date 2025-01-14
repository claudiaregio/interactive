# Using HTML and JavaScript with .NET Interactive

Creating visualizations for data is one of the key features of notebooks. In both Visual Studio Code and Jupyter, the frontend can render HTML, and there are numerous libraries available for .NET and Python the help create plots and visualizations. It's common in such libraries that you can write code that executes in the kernel, and the library handles the transformation into HTML and JavaScript for you. With .NET Interactive, there are some APIs available to simplify the process of directly writing your HTML and JavaScript. This enables you to create custom visualizations and directly access the broader ecosysytem of JavaScript libraries without needing wrapper libraries. You may choose to use these APIs directly or use them to create custom extensions to enrich the visualization of .NET types.

In this section, we'll take a look at:

* Emitting HTML
* Emitting JavaScript
* Accessing kernel data from client-side JavaScript code

## Emitting HTML

The simplest way to write some HTML to the client in .NET Interactive is to use the `#!html` magic command:

<img src="https://user-images.githubusercontent.com/547415/82240545-7245bc80-98ef-11ea-9686-7e9722ec0754.png" width="60%">

Another way to write out HTML is to display or return a value of type `IHtmlContent`, which is used to signal that a `string` should not be HTML-encoded but rather treated as valid HTML:

<img src="https://user-images.githubusercontent.com/547415/82240791-df595200-98ef-11ea-86ff-830627bb565d.png" width="60%">

The `HTML` helper method is available for wrapping a string into an `IHtmlContent` instance, which will accomplish the same thing:

<img src="https://user-images.githubusercontent.com/547415/82270446-b655b300-9929-11ea-860f-2cc80a1c20bc.png" width="60%">

A fourth approach, if you'd like to avoid thinking about string escaping and HTML encoding, and you're writing your code in C#, is to use the `PocketView` API:

<img src="https://user-images.githubusercontent.com/547415/82241257-a8377080-98f0-11ea-92e7-c6329db2d707.png" width="60%">

`PocketView` is a C# domain-specific language for writing HTML. You can learn more about it [here](pocketview.md).

## Emitting JavaScript

Just like you can directly write HTML using a magic command, you can also scripts that will be run on the frontend. The simplest approach is again a magic command, either `#!javascript` or `#!js`:

<img src="https://user-images.githubusercontent.com/547415/82244383-00bd3c80-98f6-11ea-8778-80d933a901c6.png" width="60%">

## Accessing kernel data from client-side JavaScript code

Most of the interesting work in your notebook is probably happening in the kernel, not in the client, so .NET Interactive gives you a way to access the data from the kernel. Any top-level variables declared in a kernel can be accessed from JavaScript running in the client using the `interactive` object. 

Here's an example:

<img src="https://user-images.githubusercontent.com/547415/82252142-4d5b4480-9903-11ea-8224-e044027085b6.png" width="60%">

The `interactive` object contains the following properties, corresponding to the default `dotnet-interactive` subkernels:

* `interactive.csharp`
* `interactive.fsharp`
* `interactive.pwsh`
* `interactive.value`

## Loading external JavaScript modules at runtime

Sometimes you might need to import JavaScript modules into your notebook. You can use the `interactive` object to do so.

Here is an example that configures RequireJS to load [D3.js](https://d3js.org/) from a CDN. `configureRequire` returns a function that can be used to load the module.

```js
#!js
dtreeRequire = interactive.configureRequire({
    paths: {
        d3: "https://d3js.org/d3.v5.min"
    }
});
```

You can require the module by invoking `dtreeRequire`. Then, you can use `#!html` to inject an `svg` element and then `#!js` to load and call the `d3.v5.min.js` library.


```
#!html
<svg id="renderTarget" width=300 height=300></svg>

#!js
dtreeRequire(["d3"], d3 => {
    d3.select("svg#renderTarget")
    .append('circle')
    .attr('cx', 100)
    .attr('cy', 100)
    .attr('r', 50)
    .attr('stroke', 'black')
    .attr('fill', '#69a3b2');    
});
```

The `interactive.configureRequire` is equivalent to [`require.config`](https://www.tutorialspoint.com/requirejs/requirejs_quick_guide.htm).

Doing this in .NET Interactive:

```js
#!js
dtreeRequire = interactive.configureRequire({
    paths: {
        d3: "https://d3js.org/d3.v5.min"
    }
});

dtreeRequire(["d3"], d3 => { 
    console.log(d3);
});
```

is equivalent to doing this with RequireJS:

```js
requirejs.config({
    paths: {
        d3: "https://d3js.org/d3.v5.min"
    }
});

require(["d3"], d3 => { 
    console.log(d3);
});
```
