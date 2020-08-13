---
title:  "Down the rabbit hole"
date:   2018-02-25 14:54:00 +01:00
categories: jekyll
tags: 
  - jekyll
  - jekyll themes
  - remote themes
  - github pages
  - open source
  - msys2
  - mingw
  - gcc
  - curl
  - libcurl
  - winssl
---
"Just setup jekyll on your local windows 10 machine" they said, how hard can it be?

You've installed ruby, then jekyll. Created a site, found a theme, installed some gems, bundled something and now you have some html served up on port 4000. But wait a minute, now your console is full of warnings:

### Sass 4.0 deprecated function call

> DEPRECATION WARNING: Passing a string to call() is deprecated and will be illegal in Sass 4.0. Use call(get-function("mixin-exists")) instead.

Ok, that should be easily fixed, just update the theme:

> RUNTIME DEPENDENCIES (7):
> 
> jekyll ~> 3.6

Oh right, you're running jekyll 3.5, time to upgrade! You better use ridk, and where's msys2:

> MSYS2 could not be found.
> 
> Please download and install MSYS2 from https://msys2.github.io/

Fine, I'll just install msys2:

> :: msys2-runtime and catgets are in conflict. Remove catgets? [y/N]

Ah of course, the old catgets, who invited you to the party anyway.

> Dependency Error: Yikes! It looks like you don't have jekyll-remote-theme or one of its dependencies installed. In order to use Jekyll as currently configured , you'll need to install this gem. The full error message from Ruby is: 'Could n ot open library 'libcurl': (illegal characters) . Could not open libr ary 'libcurl.dll': (illegal characters) . Could not open library 'lib curl.so.4': (illegal characters) . Could not open library 'libcurl.so .4.dll': (illegal characters) ' If you run into trouble, you can find helpful resources at https://jekyllrb.com/help/!

{% highlight markdown %}
gem install jekyll-remote-theme
make "DESTDIR=" clean
'make' is not recognized as an internal or external command, operable program or batch file.
{% endhighlight %}

Oh now we're onto a whole new world of pain. So now I'm fiddling around with pacman, on msys2, updating gcc toolchains... Right, let's get back to the job at hand and install libcurl:

{% highlight markdown %}
gem install curb -- --with-curl-lib="C:/curl-7.27.0-devel-mingw32/bin" --with-curl-include="C:/curl-7.27.0-devel-mingw32/include"
This could take a while...
ERROR: Error installing curb:
Can't find libcurl or curl/curl.h (RuntimeError)
{% endhighlight %}

We're deep in the rabbit hole now, I've forgotten why we started this journey in the first place 🤔. Some downloading and extracting of various libcurl builds later, oh and renaming libcurl-x64.dll to libcurl.dll. Finally curb has done something. Let's try again:

{% highlight markdown %}
Remote Theme: Downloading https://codeload.github.com/mmistakes/minimal-mistakes/zip/master to C:/AppData/Local/Temp/jekyll-remote-theme-20171109-11884-1vg93xx.zip
ETHON: Libcurl initialized
SSL certificate problem: unable to get local issuer certificate
{% endhighlight %}

I see, of course you have to have the latest ca-cert-bundle.crt, oh and don't forget to set the environment variables:

{% highlight markdown %}
CURL_CA_BUNDLE=C:\Ruby\bin\ca-bundle.crt
SSL_CERT_FILE=C:\Ruby\bin\cacert.pem
{% endhighlight %}

Try again:

> SSL certificate problem: unable to get local issuer certificate

Oh right, still doesn't work. What exactly are those environment variables doing anyway:

[http://www.rubydoc.info/gems/ethon/0.5.0/Ethon/Easy](http://www.rubydoc.info/gems/ethon/0.5.0/Ethon/Easy)
> The CURLOPT_CAPATH function apparently does not work in Windows due to some limitation in openssl.

[https://stackoverflow.com/questions/37551409/configure-curl-to-use-default-system-cert-store](https://stackoverflow.com/questions/37551409/configure-curl-to-use-default-system-cert-store)
> OpenSSL does not support using the "CA certificate store" that Windows has on its own. If you want your curl build to use that cert store, you need to rebuild curl to use the schannel backend instead

This could be the end of the adventure, we could either download the OpenSSL source and rebuild on windows with WinSSL or find someone who has already built libcurl with WinSSL. Wait a minute, what's this:

[https://skanthak.homepage.t-online.de/curl.html](https://skanthak.homepage.t-online.de/curl.html)
> X:\> i386\curl.exe -V
> 
> curl 7.58.0 (i386-pc-win32) libcurl/7.58.0 WinSSL zlib/1.2.11

Finally, 2 days later and I have a functioning jekyll setup on windows 10 with libcurl and jekyll-remote-theme. That was really fun 🤮. 