window.syntaxHighlight = () => 
    document.querySelectorAll('pre code').forEach((block) => {
        hljs.highlightBlock(block);
    });