all:
	latexmk -pdf report.tex

clean:
	latexmk -c
