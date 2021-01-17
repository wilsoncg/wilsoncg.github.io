window.chat = () => 
    {
        (async function() {
            const res = await fetch('https://rachaelazurefunction.azurewebsites.net/api/Token', 
              { method: 'POST' });
            const { token } = await res.json();
    
            window.WebChat.renderWebChat(
              {
                directLine: await window.WebChat.createDirectLine({ token })
              },
              document.getElementById('webchat')
            );
    
            document.querySelector('#webchat > *').focus();
          })().catch(err => console.error(err));
    };