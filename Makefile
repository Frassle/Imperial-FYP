all:
	latexmk -pdf report.tex
	latexmk -pdf typeEquality.tex

diff:
	latexdiff submission.tex report.tex > diff.tex
	latexmk -pdf diff.tex
	rm diff.tex

clean:
	latexmk -c
