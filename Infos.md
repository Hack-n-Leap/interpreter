Informations pour initialiser :
(test = nom de l'instance)

Variables :
	var test = 5
		--> crée une variable test avec pour valeur 5
	var test = "ceci est un test"
		--> crée une variable test avec pour valeur "ceci est un test"

Affichage :
	print 5
		--> affiche 5 dans la sortie

	autres exemples :

	print "ceci est un test"
		--> ceci est un test
	print test
		--> ceci est un test

Fonctions :
	func test(x, y, ..) {
		commandes
	}
	--> crée une fonction nommée test qui prend pour paramêtres x, y, .. et execute les commandes
	
	test(5, 10, ..) 
	--> appelle la fonction test avec 5 (=x), 10(=y), .. comme paramêtres


	exemples :
	
	func test(toto) {
		print toto
		print "ceci est une fonction qui affiche la valeur de toto"
	}
	test("ceci est un test")
	
	--> ceci est un test
	--> ceci est une fonction qui affiche la valeur de toto
	
Opérations :
	Addition : +
	Soustraction : -
	Multiplication : *
	Division : /
	Puissance : ^
	
	/!\ mettre des parenthèse si multiplications ou divisions
	+ ne pas coller les chiffre/variables aux opérateurs
	
	exemple :

	var a = 5 + 6
	print a * (1 + 9)
	print (a - 1) / (10 - (5 + 3))
	print 2 ^ 3

	--> 110
	--> 5
	--> 8


	