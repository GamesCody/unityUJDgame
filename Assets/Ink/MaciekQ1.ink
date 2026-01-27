// --- ZMIENNE STANU ---
VAR laptopReturned = false
VAR maciekWait = false

// --- GŁÓWNY PRZEPŁYW ---
-> main

=== main ===
{ laptopReturned:
    -> scena_dziekuje_za_pomoc
- else:
    -> scena_1_konfrontacja
}

// --- WĘZŁY FABULARNE ---

=== scena_dziekuje_za_pomoc ===
Maciek widzi Cię i uśmiecha się.
# speaker: narrator

Hej! Dziękuję Ci naprawdę za pomoc!
# speaker: maciek

Udało mi się przepisać cały kod na Linuxa. Profesor będzie z siebie zadowolony.
# speaker: maciek

Bez Ciebie nigdy bym tego nie zrobił. Jesteś legendą!
# speaker: maciek

Czekaj… może Ci się jeszcze do czegoś przydać?
# speaker: maciek

-> END

=== scena_1_konfrontacja ===
{ maciekWait:
    -> sprawdz_laptop
- else:
    Gracz wchodzi do klasy i jest świadkiem następującej wymiany zdań.
    # speaker: narrator

    Proszę pana profesora, zrobiłem program.
    # speaker: maciek

    Doskonale, doskonale... proszę mi pokazać... a co to jest?
    # speaker: profesor

    Program, program potrafi...
    # speaker: maciek

    Nie pytam o to! Co to za system?!
    # speaker: profesor

    Eeee...
    # speaker: maciek

    CO TO ZA SYSTEM?!
    # speaker: profesor

    W-windows... Windows, proszę pana.
    # speaker: maciek

    Jaki Windows?! JAKI WINDOWS?! TY GŁUPCZE!
    # speaker: profesor

    Tylko Linux!
    # speaker: profesor

    Ile razy mam powtarzać?!
    # speaker: profesor

    Ale panie profesorze, to tylko... system.
    # speaker: maciek

    Agh... augh!
    # speaker: profesor

    (łapie się za pierś)
    # speaker: narrator

    Moje serce... moje serce!
    # speaker: profesor

    CO TY POWIEDZIAŁEŚ?!
    # speaker: profesor

    T-to tylko sys...
    # speaker: maciek

    NIE!
    # speaker: profesor

    Zamilcz!
    # speaker: profesor

    Ale profesorze...
    # speaker: maciek

    Jaki „profesorze"?!
    # speaker: profesor

    Ja już...
    # speaker: profesor

    Ja już nie mam ucznia.
    # speaker: profesor

    ODEJDŹ!!!
    # speaker: profesor

    Odejdź...
    # speaker: profesor

    Stary… on zaraz zejdzie na zawał, a ja wylecę ze szkoły.
    # speaker: maciek

    Ten kod to moje życie, ale on nie tknie niczego, co nie jest napisane pod Linuxem.
    # speaker: maciek

    Pomożesz mi?
    # speaker: maciek

    Trzeba przepisać biblioteki, zmienić ścieżki i skompilować to na jego ulubionej dystrybucji…
    # speaker: maciek

    Zanim on wezwie karetkę albo komisję dyscyplinarną.
    # speaker: maciek
    
    ~ maciekWait = true
    -> END
}

=== sprawdz_laptop ===
{ laptopReturned:
    -> scena_dziekuje_za_pomoc
- else:
    Maciek nerwowo zerka na zegarek.
    # speaker: narrator

    Pośpiesz się z tym laptopem!
    # speaker: maciek
    
    -> END 
}