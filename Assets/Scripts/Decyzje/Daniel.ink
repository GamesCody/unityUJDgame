VAR czy_zna_plan = false
VAR liczba_swiec = 0
->korytarz_spotkanie
=== korytarz_spotkanie ===
// Sprawdzamy, czy gracz już raz rozmawiał z chłopakami
{ czy_zna_plan:
    -> dialog_loop
- else:
    -> pierwszy_dialog
}

=== pierwszy_dialog ===
Na szkolnym korytarzu wpadasz na Karola i Daniela. Obaj wyglądają, jakby nie spali od co najmniej trzech patchy systemowych.
# speaker: narrator

Stary, jesteś pewien? To zakazana wiedza... Nikt tego nie robił od czasów sesji w 2012!
# speaker: Daniel

Nie mamy wyjścia! Albo to, albo warunek!
# speaker: Karol

O czym wy mówicie?
# speaker: Ty

Jak to o czym? O projekcie z gier komputerowych na jutro! Jesteś gotowy?
# speaker: Daniel

(zszokowany) Jaki projekt?!
# speaker: Ty

Nie panikuj. Mamy plan. Genialny plan. Ty pomożesz nam zebrać składniki, a my zajmiemy się… logistyką metafizyczną.
# speaker: Karol

Ogarnij nam świece porozrzucane po mapie. Konkretnie 3, wtedy wróć.
# speaker: Karol

~ czy_zna_plan = true
-> END

=== dialog_loop ===
{ liczba_swiec < 3:
    Znajdź wszystkie świece, potrzebujemy ich! Mamy ich obecnie {liczba_swiec}/3.
    # speaker: Daniel
- else:
    Masz wszystkie trzy? Idealnie... Czas zacząć rytuał kompilacji!
    # speaker: Karol
    // Tutaj możesz dodać przejście do kolejnej sceny
}
-> END