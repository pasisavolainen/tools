# dtail - docker tail

The purpose of this tool is to show logs of selected containers, formatted mostly like IRC
```IRC log
12:34.02 <log> Starting up..
12:34.50 <log> Done
12:35.20 <file> Receiving hello.c
```

# features wanted

 - [x] selecting visible containers
 - [x] store selections/settings in config file (`~/.dtailrc.json`)
 - [x] rename containers (visible shortname, alias)
 - [ ] log cursor position retainer (don't scroll off with the logs)
 - [ ] custom highlight strings ("red" for exceptions ..)
 - [ ] quick single container override (show single container log)
 - [ ] autoreplace certain formats (dates, warn/info/debug, tag container f.ex "aspnet")
 - [ ] keyword search into "channel" -> CorrelationId
 - [ ] spamminess throttle (cleanup ~n recent repeating lines)
 - [ ] group containers to "channels"
 - [ ] ping (^G) on keyword

# needfix

- [ ] backbuffer grows without limit
- [ ] initial line order is by container found, not chrono
- [ ] as containers die the Task gets stale
- [ ] no duplicate containername/alias situation resolution
- [ ] window resize breaks statusbar
- [ ] handle long lines
- [ ] special chars (f.ex color VT codes) are encoded into character soup
- [ ] need to reattach container history when it goes down (ProgressTask dies) and then starts up again
- [ ] investigate why DockerNet fails to deliver Monitor messages