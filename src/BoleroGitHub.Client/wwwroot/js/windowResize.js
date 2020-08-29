window.generalFunctions = {
    env: {
      rate: 1000,
      lastResize: 0
    },
    getSize: function(){
      var size = { "height": window.innerHeight, "width" : window.innerWidth };
      if (Date.now() - this.env.lastResize >= this.env.rate) {
        size.height = window.innerHeight;
        size.width = window.innerWidth;
        this.env.lastResize = Date.now();
      }
      return size;
    },
    handleEvent: function() {
          return this[event.type](event); // dispatch to method with name of event
    },
    resize: function(event){
      var size = this.getSize();
      //dotnetHelper.invokeMethodAsync('BoleroGitHub.Client','Main.MyApp.OnWindowResizeAsync', size.width, size.height);
    },
    initResizeCallback: function(onResize) {
      console.log("initResizeCallback");
      window.addEventListener('resize', (ev) => { 
        this.resizeCallbackJS(onResize);
      });
    },
    resizeCallbackJS: function(callback) {
      console.log("resizeCallbackJS triggered");
      var size = this.getSize();
      callback.invokeMethodAsync('Invoke', size.height, size.width);
      // dispose needed?
      // callback.dispose();
    },
    registerResize: function() {
      window.addEventListener('resize', this);
    },
    unregisterResize: function() {
      window.removeEventListener('resize', this);
    }
  };
  
  //window.generalFunctions.registerResize();