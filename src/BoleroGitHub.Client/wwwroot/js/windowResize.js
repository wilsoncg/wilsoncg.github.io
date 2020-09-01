window.generalFunctions = {
    env: {
      rate: 1000,
      lastResize: 0,
      hamburgerVisible: false
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
    },
    initResizeCallback: function(onResize) {
      window.addEventListener('resize', (ev) => {         
        this.resizeCallbackJS(onResize);
      });
    },
    resizeCallbackJS: function(callback) {
      var size = this.getSize();
      if(size.width < 450 && !this.env.hamburgerVisible)
      {
        this.env.hamburgerVisible = true;
        callback.invokeMethodAsync('Invoke', size.height, size.width);
      }
      if(size.width > 450 && this.env.hamburgerVisible)
      {
        this.env.hamburgerVisible = false;
        callback.invokeMethodAsync('Invoke', size.height, size.width);
      }
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