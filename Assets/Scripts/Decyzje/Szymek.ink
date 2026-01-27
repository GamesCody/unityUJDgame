VAR ma_zapalki = false
VAR czy_zna_szymka = false
->scena_szymek
=== scena_szymek ===
// Sprawdzamy, czy gracz już raz rozmawiał z chłopakami
{ czy_zna_szymka:
    -> dialog_loop
- else:
    -> pierwszy_dialog
}

=== pierwszy_dialog ===
# speaker: Ty
Hej Szymek, masz może zapałki? Potrzebujemy ich do… projektu.
~czy_zna_szymka=true
# speaker: Szymek
Zapałki? Ja? Stary, ja nie palę. Rzuciłem. Definitywnie. Od trzech godzin i czterdziestu minut ani jednego dymka. Czysty jak łza, organizm jak świątynia.

# speaker: Ty
Szymek, widzę dym lecący z twojego rękawa.

# speaker: Szymek
CO?! Procesor ci się przegrzewa chyba!
(Szymek nerwowo wachluje rękawem, patrząc w sufit)
...
->dialog_loop
=== dialog_loop ===
{ ma_zapalki:
    # speaker: Szymek
    Ah, oszukujesz!  Więcej z tobą nie gram.
    -> END
}
Dobra… mam zapałki. Ale nie dam ich byle komu. Zagramy w „Kamień, Papier, Nożyce”.
+ [No dobra, gramy.]
    # speaker: Szymek
    Super. Tylko szybko, bo mi się procesor grzeje... Raz, dwa, trzy!
    ~ma_zapalki = true
    -> END

+ [Nie gram, nie mam czasu.]
    # speaker: Szymek
    Twoja strata. Ale jak zmienisz zdanie i będziesz potrzebował ognia, to wiesz, gdzie mnie szukać. (Szymek znowu nerwowo wachluje rękaw)
    -> END