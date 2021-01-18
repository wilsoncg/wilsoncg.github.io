window.chat = () => 
    {
        (async function() {
            const res = await fetch('https://rachaelazurefunction.azurewebsites.net/api/Token', 
              { method: 'POST' });
            const { token } = await res.json();

            const store = window.WebChat.createStore({}, ({ dispatch }) => next => action => {
              if (action.type === 'DIRECT_LINE/INCOMING_ACTIVITY') {
                const event = new Event('webchatincomingactivity');
    
                event.data = action.payload.activity;
                window.dispatchEvent(event);
              }
    
              return next(action);
            });
    
            window.WebChat.renderWebChat(
              {
                directLine: await window.WebChat.createDirectLine({ token }),
                store
              },
              document.getElementById('webchat')             
            );

            window.addEventListener('webchatincomingactivity', ({ data }) => {
              document.getElementById('main-content').scrollIntoView(false);
            });
    
            document.querySelector('#webchat > *').focus();
          })().catch(err => console.error(err));
    };