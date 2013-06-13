all:
	latexmk -pdf report.tex

diff:
	latexdiff submission.tex report.tex > diff.tex
	latexmk -pdf diff.tex
	rm diff.tex

clean:
	latexmk -c
