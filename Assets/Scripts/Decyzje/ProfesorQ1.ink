->scena_profesor_nagroda
VAR poprawkaZGrafow = false
VAR walkaBoss1 = false
// --- WĘZEŁ PROFESORA ---

=== scena_profesor_nagroda ===
{ poprawkaZGrafow:
    -> rozmowa_o_poprawce
- else:
    Profesor mruczy coś pod nosem, patrząc w monitor z terminalem Linuxa. 
    # speaker: narrator
    
    Nie widzi pan, że kompiluję jądro?! Proszę mi nie przeszkadzać!
    # speaker: profesor
    -> END
}

=== rozmowa_o_poprawce ===
Profesor poprawia okulary i patrzy na Ciebie znad monitora. Już nie wygląda, jakby miał mieć zawał.
# speaker: narrator

A, to Ty. Maciek przyniósł mi ten program. 
# speaker: profesor

Muszę przyznać... kod jest czysty, ścieżki poprawne, a co najważniejsze – skompilowany pod Linuxem, tak jak Pan Bóg przykazał.
# speaker: profesor

Wiem, że to Twoja sprawka. Maciek sam by na to nie wpadł, on ma Windowsa w głowie...
# speaker: profesor

Słuchaj... doceniam szerzenie kultury Open Source. Twoje ostatnie kolokwium z teorii grafów było... powiedzmy, że dyskusyjne.
# speaker: profesor

Chcesz dostać szansę na poprawkę w podzięce za uratowanie tego chłopaka przed kompromitacją?
# speaker: profesor

+[Tak, panie profesorze. Bardzo chętnie.]
    To dobrze. Cenię ambicję. Przyjdź do mnie w przyszłym tygodniu, przygotuj algorytm Dijkstry i strukturę drzew rozpinających.
    # speaker: profesor
    # bossFightTriggered
    ~ walkaBoss1 = true
-> END

+ [Nie, dziękuję. Moja ocena mi wystarczy.]
    Twoja strata. Ale przynajmniej masz zasady. Wracaj do pracy, tylko nie dotykaj niczego, co ma logo Microsoftu.
    # speaker: profesor
    -> END