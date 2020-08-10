---
title:  "Jekyll up and running"
date:   2017-07-09 16:36:51 +0100
categories: jekyll
tags: 
  - jekyll
  - front matter
  - getting started
  - github pages
---
Jekyll is a tool to create static web pages generated from markdown files. This means you don't have to worry about paying for wordpress hosting, or worse setting up a LAMP server, with MySQL/PHP just to host a blog. It's got some really useful features, such as syntax highlighting for all the languages that github supports:

{% highlight csharp %}
public void Hello(string name)
{
    Action<string> helloThere = (n) => 
    {
      Console.WriteLine($"Hi, {n}");
    };
    helloThere(name);
}
Hello("Tom")
#=> prints 'Hi, Tom' to the Console.
{% endhighlight %}

Check out the [Jekyll docs][jekyll-docs] for more info on how to use Jekyll.

[jekyll-docs]: https://jekyllrb.com/docs/home
