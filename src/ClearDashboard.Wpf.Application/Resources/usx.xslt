<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output indent="yes" />
  <xsl:strip-space elements="*" />

  <xsl:template match="usx">
    <html>
      <head>
        <meta content="utf-8" />
        <!-- <link rel="stylesheet" href="bible.css" /> -->
        <style>
          body {
          }
          /* HTML5 display-role reset for older browsers */
          font-size: 20px;
          font-family: "Noto Serif", serif;
          line-height: 25px;

          -webkit-font-smoothing: antialiased;

          html,
          body,
          div,
          span,
          applet,
          object,
          iframe,
          h1,
          h2,
          h3,
          h4,
          h5,
          h6,
          p,
          blockquote,
          pre,
          a,
          abbr,
          acronym,
          address,
          big,
          cite,
          code,
          del,
          dfn,
          em,
          img,
          ins,
          kbd,
          q,
          s,
          samp,
          small,
          strike,
          strong,
          sub,
          sup,
          tt,
          var,
          b,
          u,
          i,
          center,
          dl,
          dt,
          dd,
          ol,
          ul,
          li,
          fieldset,
          form,
          label,
          legend,
          table,
          caption,
          tbody,
          tfoot,
          thead,
          tr,
          th,
          td,
          article,
          aside,
          canvas,
          details,
          embed,
          figure,
          figcaption,
          footer,
          header,
          hgroup,
          menu,
          nav,
          output,
          ruby,
          section,
          summary,
          time,
          mark,
          audio,
          video {
          margin: 0;
          padding: 0;
          border: 0;
          font-family: inherit;
          vertical-align: baseline; }
          article,
          aside,
          details,
          figcaption,
          figure,
          footer,
          header,
          hgroup,
          menu,
          nav,
          section {
          display: block; }
          ol,
          ul {
          list-style: none; }
          blockquote,
          q {
          quotes: none; }
          blockquote:before,
          blockquote:after,
          q:before,
          q:after {
          content: "";
          content: none; }
          table {
          border-collapse: collapse;
          border-spacing: 0; }

          .c {
          text-align: center;
          font-weight: bold;
          font-size: 1.3em; }

          .ca {
          font-style: italic;
          font-weight: normal;
          color: #777777; }
          .ca:before {
          content: "("; }
          .ca:after {
          content: ")" !important; }

          .cl {
          text-align: center;
          font-weight: bold; }

          .cd {
          margin-left: 1em;
          margin-right: 1em;
          font-style: italic; }

          .p {
          font-size: 17px;
          }

          .v,
          .verse,
          .vp,
          sup[class^="v"] {
          color: #3399ff;
          background: #dedede
          font-size: 0.7em;
          letter-spacing: -0.03em;
          vertical-align: 0.25em;
          line-height: 1.5;
          font-family: sans-serif;
          top: inherit; }
          .v:after,
          .vp:after,
          sup[class^="v"]:after {
          content: "\a0"; }

          sup + sup:before {
          content: "\a0"; }

          sup + sup:before {
          content: "\a0"; }

          .va {
          font-style: italic; }
          .va:before {
          content: "("; }
          .va:after {
          content: ")" !important; }

          .notelink {
          text-decoration: underline;
          padding: 0.1em;
          }

          .notelink,
          .notelink:hover,
          .notelink:active,
          .notelink:visited {
          color: #6a6a6a;
          }

          .notelink sup {
          font-size: 0.7em;
          letter-spacing: -0.03em;
          vertical-align: 0.25em;
          line-height: 0;
          font-family: sans-serif;
          font-weight: bold;
          }

          .notelink+sup:before {
          content: "\a0";
          }

          .xhead {
          font-size: 11px;
          color: #008fff;
          }

          .x {
          display: none;
          width: calc(98vw - 35px);
          background: #ffffa1;
          border-radius: 6px;
          padding: 5px 5px;
          left: 10px;
          border: 2px solid grey;
          line-height: normal;
          text-decoration: none;
          position: absolute;
          z-index: 1;

          font-size: 16px;
          color: #000;
          text-align: center;
          }

          .xhead:hover .x {
          display: block;
          }

          .xo {
          font-weight: bold;
          }

          .xk {
          font-style: italic;
          }

          .xq {
          font-style: italic;
          }

          .fhead {
          font-size: 11px;
          color: #008fff;
          }

          .f {
          display: none;
          width: calc(98vw - 35px);
          background: #ffffa1;
          border-radius: 6px;
          padding: 5px 5px;
          left: 10px;
          border: 2px solid grey;
          line-height: normal;
          text-decoration: none;
          position: absolute;
          z-index: 1;

          font-size: 16px;
          color: #000;
          text-align: center;
          }

          .fhead:hover .f {
          display: block;
          }

          .fr {
          font-weight: bold; }

          .fk {
          font-style: italic;
          font-variant: small-caps; }

          [class^="fq"] {
          font-style: italic; }

          .fl {
          font-style: italic;
          font-weight: bold; }

          .fv {
          color: #515151;
          font-size: 0.75em;
          letter-spacing: -0.03em;
          vertical-align: 0.25em;
          line-height: 0;
          font-family: sans-serif;
          font-weight: bold; }
          .fv:after {
          content: "\a0"; }

          .h {
          text-align: center;
          font-style: italic; }

          [class^="imt"],
          [class^="is"] {
          text-align: center;
          font-weight: bold;
          font-size: 20px;
          line-height: 50px;
          margin-top: 25px;
          margin-bottom: 25px; }

          [class^="ip"] {
          text-indent: 1em; }

          .ipi {
          padding-left: 1em;
          padding-right: 1em; }

          .im {
          text-indent: 0; }

          .imi {
          text-indent: 0;
          margin-left: 1em;
          margin-right: 1em; }

          .ipq {
          font-style: italic;
          margin-left: 1em;
          margin-right: 1em; }

          .imq {
          margin-left: 1em;
          margin-right: 1em; }

          .ipr {
          text-align: right;
          text-indent: 0; }

          [class^="iq"] {
          margin-left: 1em;
          margin-right: 1em; }

          .iq2 {
          text-indent: 1em; }

          [class^="ili"] {
          padding-left: 1em;
          text-indent: -1em; }

          .ili1 {
          margin-left: 1em;
          margin-right: 1em; }

          .ili2 {
          margin-left: 2em;
          margin-right: 1em; }

          .iot {
          font-weight: bold;
          font-size: 18px;
          line-height: 25px;
          margin-top: 25px;
          text-align: center;
          margin-bottom: 0px; }

          .io,
          .io1 {
          margin-left: 1em;
          margin-right: 0em; }

          .io2 {
          margin-left: 2em;
          margin-right: 0em; }

          .io3 {
          margin-left: 3em;
          margin-right: 0em; }

          .io4 {
          margin-left: 4em;
          margin-right: 0em; }

          .ior {
          font-style: italic; }

          .iex {
          text-indent: 1em; }

          .iqt {
          text-indent: 1em;
          font-style: italic; }

          [class^="p"] {
          text-indent: 1em; }

          .m {
          text-indent: 0 !important; }

          .pmo {
          text-indent: 0;
          margin-left: 1em;
          margin-right: 0em; }

          .pm {
          margin-left: 1em;
          margin-right: 0em; }

          .pmr {
          text-align: right; }

          .pmc {
          margin-left: 1em;
          margin-right: 0em; }

          .pi {
          margin-left: 1em;
          margin-right: 0em; }

          .pi1 {
          margin-left: 2em;
          margin-right: 0em; }

          .pi2 {
          margin-left: 3em;
          margin-right: 0em; }

          .pi3 {
          margin-left: 4em;
          margin-right: 0em; }

          .mi {
          margin-left: 1em;
          margin-right: 0em;
          text-indent: 0; }

          .pc {
          text-align: center;
          text-indent: 0; }

          .cls {
          text-align: right; }

          [class^="li"] {
          padding-left: 1em;
          text-indent: -1em;
          margin-left: 1em;
          margin-right: 0em; }

          .li2 {
          margin-left: 2em;
          margin-right: 0em; }

          .li3 {
          margin-left: 3em;
          margin-right: 0em; }

          .li4 {
          margin-left: 4em;
          margin-right: 0em; }

          .b {
          height: 25px; }

          [class^="q"] {
          padding-left: 1em;
          text-indent: -1em;
          margin-left: 1em;
          margin-right: 0em; }

          .q2 {
          margin-left: 1.5em;
          margin-right: 0em; }

          .q3 {
          margin-left: 2em;
          margin-right: 0em; }

          .q4 {
          margin-left: 2.5em;
          margin-right: 0em; }

          .qr {
          text-align: right;
          font-style: italic; }

          .qc {
          text-align: center; }

          .qs {
          font-style: italic;
          text-align: right; }

          .qa {
          text-align: center;
          font-style: italic;
          font-size: 1.1em;
          margin-left: 0em;
          margin-right: 0em; }

          .qac {
          margin-left: 0em;
          margin-right: 0em;
          padding: 0;
          text-indent: 0;
          font-style: italic; }

          .qm2 {
          margin-left: 1.5em;
          margin-right: 0em; }

          .qm3 {
          margin-left: 2em;
          margin-right: 0em; }

          .qt {
          font-style: italic;
          text-indent: 0;
          padding: 0;
          margin: 0; }

          .bk {
          font-style: italic; }

          .nd {
          font-variant: small-caps; }

          .add {
          font-style: italic; }

          .dc {
          font-style: italic; }

          .k {
          font-weight: bold;
          font-style: italic; }

          .lit {
          text-align: right;
          font-weight: bold; }

          .pn {
          font-weight: bold;
          text-decoration: underline; }

          .sls {
          font-style: italic; }

          .tl {
          font-style: italic; }

          .wj {
          color: #cc0000; }

          .em {
          font-style: italic; }

          .bd {
          font-weight: bold; }

          .it {
          font-style: italic; }

          .bdit {
          font-weight: bold;
          font-style: italic; }

          .no {
          font-weight: normal;
          font-style: normal; }

          .sc {
          font-variant: small-caps; }

          .qt {
          font-style: italic; }

          .sig {
          font-weight: normal;
          font-style: italic; }

          table {
          width: 100%;
          display: table; }

          .tr {
          display: table-row; }

          [class^="th"] {
          font-style: italic;
          display: table-cell; }

          [class^="thr"] {
          text-align: right;
          padding: 0 25px; }

          [class^="tc"] {
          display: table-cell; }

          [class^="tcr"] {
          text-align: right;
          padding: 0 25px; }

          [class^="mt"] {
          text-align: center;
          font-weight: bold;
          letter-spacing: normal; }

          .mt,

          .mt1 {
          font-size: 30px;
          line-height: 50px;
          text-align: center;
          margin-top: 25px;
          margin-bottom: 25px; }
          .mt2 {
          font-size: 20px;
          line-height: 50px;
          text-align: center;
          margin-top: 25px;
          font-style: italic;
          margin-bottom: 25px; }

          [class^="ms"],
          .ms,
          .ms1,
          .ms2,
          .ms3 {
          text-align: center;
          font-weight: bold;
          font-size: 27px;
          line-height: 50px;
          margin-top: 25px;
          margin-bottom: 0px; }

          .mr {
          font-size: 0.9em;
          margin-bottom: 25px;
          text-align: center;
          font-weight: normal;
          font-style: italic; }

          .s,
          .s1,
          .s2,
          .s3,
          .s4 {
          text-align: center;
          color: #db7d1f;
          font-size: 18px;
          font-weight: bold;
          font-style: italic;
          line-height: 50px;
          margin-bottom: 25px;
          margin-top: 0px; }

          .sr {
          font-weight: normal;
          font-style: italic;
          text-align: center;
          font-size: inherit;
          letter-spacing: normal; }

          .r {
          font-size: 18px;
          font-weight: normal;
          font-style: italic;
          text-align: center;
          letter-spacing: normal; }

          .rq {
          font-size: inherit;
          line-height: 25px;
          font-style: italic;
          text-align: right;
          letter-spacing: normal; }

          .d {
          font-style: italic;
          text-align: center;
          font-size: inherit;
          letter-spacing: normal; }

          .sp {
          text-align: left;
          font-weight: normal;
          font-style: italic;
          font-size: inherit;
          letter-spacing: normal; }

          .heb {
          color: #08450a;
          font-weight: bold;
          font-style: italic; }

          .scr {
          color: #7d0606;
          font-weight: bold;
          font-style: normal;}

          .ver {
          font-family: "Courier";
          font-weight: bold;
          font-style: italic;
          color:blue;
          }

          .v {
          background-color:#e3e3e3;
          font-weight: bold;
          vertical-align: super;
          font-size:6px;
          font-style: normal;}

          .vh {
          background-color:#ffffa1;}

          .res {
          font-weight: bold;
          font-style: normal;}

          .teu {
          font-style: normal;
          text-decoration: underline;}

          .tec {
          font-weight: bold;
          font-style: normal;}

          .s5 {
          font-color: #030ffc
          font-weight: bold;
          font-style: normal;}

          .tc1 {
          border: 2px solid black;
          padding: 5px;
          font-weight: bold;
          font-style: normal;
          }

          .tc2 {
          border: 1px solid black;
          padding: 5px;
          font-weight: bold;
          font-style: normal;
          }

          .sl1 {
          font-weight: bold;
          }

          .mlor {
          font-style: italic;
          font-variant: small-caps;
          }

          .imp {
          font-style: normal;
          color:gray;
          }

          .brk {
          font-style: normal;
          vertical-align: sub;
          font-size: smaller;
          }

          .rgm{
          vertical-align: super;
          font-size: smaller;
          }

          html {
          scroll-padding-top: 60px; /* height of sticky header*/
          }

          .navbar {
          overflow-x: auto;
          display: flex;
          background-color: white;
          top: 0; /* Position the navbar at the top of the page */
          width: 100%; /* Full width */
          position: sticky; /* Stay in place */
          }

          /* Links inside the navbar */
          .navbar a {
          margin-right:5;
          background-color: #333;
          border-radius: 25px;
          float: left;
          color: #f2f2f2;
          text-align: center;
          padding: 12px 12px;
          text-decoration: none;
          font-size: 12px;
          }

          /* Change background on mouse-over */
          .navbar a:hover {
          background: #ddd;
          color: black;
          }

          .navbar .active{
          background: #aaa;
          color: black;
          }

          summary{
          vertical-align:text-top;
          font-size:23px;
          color:#0069C0;
          }

          .w{
          margin-left:4px;
          }

          .text{}

          .material-icons{
          font-size:20px;
          color:#C79100;}


        </style>
      </head>
      <body>
        <main dir="auto">
          <xsl:apply-templates />
        </main>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="@style">
    <xsl:attribute name="class">
      <xsl:value-of select="."/>
    </xsl:attribute>
  </xsl:template>


  <xsl:template match="para[@style='rem']|para[@style='ide']|verse[@eid]"></xsl:template>

  <xsl:template match="para">
    <p dir="auto">
      <xsl:apply-templates select="@*|node()" />
    </p>
  </xsl:template>

  <xsl:template match="char">
    <em>
      <xsl:apply-templates select="@*|node()" />
    </em>
  </xsl:template>

  <xsl:template match="note">
    <span class="note">
      <xsl:apply-templates select="@*|node()" />
    </span>
  </xsl:template>

  <xsl:template match="chapter">
    <b class="c">
      <xsl:value-of select="@number" />
    </b>
  </xsl:template>

  <xsl:template match="book">
    <div>
      <xsl:attribute name="id">
        <xsl:value-of select="@code" />
      </xsl:attribute>
    </div>
  </xsl:template>

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()" />
    </xsl:copy>
  </xsl:template>

  <xsl:template match="node()[count(preceding::verse[@style='vh'])>=1 and count(following::verse[@style='vh'])>=1]">
    <span class="vh">
      <xsl:copy>
        <xsl:apply-templates select="@*|node()" />
      </xsl:copy>
    </span>
  </xsl:template>

  <xsl:template match="node()[@style='f']">
    <sup class="fhead">
      fnote_ 
      <xsl:copy>
        <xsl:apply-templates select="@*|node()" />
      </xsl:copy>
    </sup>
  </xsl:template>

  <xsl:template match="node()[@style='x']">
    <sup class="xhead">
      xref_
      <xsl:copy>
        <xsl:apply-templates select="@*|node()" />
      </xsl:copy>
    </sup>
  </xsl:template>

  <xsl:template match="verse[@sid]">
    <sup class="verse">
      <xsl:attribute name="id">
        <!--put your logic in variable-->
        <xsl:variable name="str">
          <xsl:value-of select="@sid" />
        </xsl:variable>
        <!--normalize space will prevent spaces from left and right of string, then all spaces inside will be replaced by '-' -->
        <xsl:value-of select="translate(normalize-space($str), ' ', '-')"/>
      </xsl:attribute>

      <xsl:value-of select="@number" />
    </sup>
  </xsl:template>

</xsl:stylesheet>