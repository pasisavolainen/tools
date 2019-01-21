
## dockertail.py

# Rationale
`docker-compose logs` kept crapping out, so this was the answer. Besides,
it doesn't really support service filtering and clean-naming so there is that.

# Example

Start `dockertail.py` with wanted containers as parameters (rename container with `,outputname`)
```sh
psi@derp:tools$ ./dockertail.py foo,foosays bar
```

Run these containers to generate output:
```sh
psi@derp:tools$ docker run --rm --name foo -d alpine sh -c "sleep 10; echo a; sleep 5; echo b; sleep 5; echo c; sleep 3"
26b9093c14aeceaa064495215489ad51ae9f9829676e3d43b54044a9cee18b77
psi@derp:tools$ docker run --rm --name bar -d alpine sh -c "sleep 10; echo a; sleep 5; echo b; sleep 5; echo c; sleep 3"
f5937e8950dd26f2d32ffdeaefb685a85eb2f46661225b264ea0fea69323eafc
```

Expected output looks like:
```
190121 14:20:10 foosays: a
190121 14:20:15 foosays: b
190121 14:20:16 bar: a
190121 14:20:20 foosays: c
190121 14:20:21 bar: b
190121 14:20:26 bar: c
```

Ctrl-C to exit