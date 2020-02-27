---
title:  "Docker plus Jekyll equals win"
date:   2020-02-26 12:00:00 +0100
categories: jekyll
tags: 
  - jekyll
  - docker
  - github pages
---
I've started to spend more time looking at docker. A useful excerise has been to use the docker jekyll image to setup a working development environment. 

[Previously]({% post_url 2018-02-25-down-the-rabbit-hole %}), it took me 2 days of configuring my windows 10 desktop with:
* Correct version of Ruby installed
* Ruby gems installed & configured correctly
* Make installed (in the event you need to modify & rebuild some part of the toolchain)
* Appropriate version of libcurl installed & setup

At any point in the setup process you will encounter an error, attempt to use google to get more context, scroll through various github issues & stackoverflow posts, and then try various combinations of command line arguments until each error is resolved.

Fast forward 6 months, a new OS update is applied which uknowingly touches one of those components and the whole stack of cards comes crashing down. In the software development world we are all too familiar with [yak shaving](https://en.wiktionary.org/wiki/yak_shaving). 

Fortunately this problem has been solved. By using automation and the [docker toolchain](https://ddewaele.github.io/running-jekyll-in-docker/), the process now becomes a 2 hour task:
* Install docker
* Learn docker
* Run 2 docker commands
* Start developing

You could argue learning docker is yet more yak shaving, but the benefit to this learning, is that docker becomes another tool in the toolbox which can hugely shorten the software development and delivery lifecycle. 