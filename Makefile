all:
	latexmk -pdf report.tex

pdf:
	git pull
	make all
	git add report.pdf
	git commit -m "New pdf"
	git push

diff:
	latexdiff submission.tex report.tex > diff.tex
	latexmk -pdf diff.tex
	rm diff.tex

clean:
	latexmk -c
