using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearDashboard.DataAccessLayer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGrammarTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('fe86171a-fd7a-46ed-9826-945eb02170a9','[1st]', '1st Person');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('9820ccd0-2e5c-4b54-b147-2b32a03f14ad','[2nd]', '2nd Person');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('237d54a6-359b-429b-bb64-bddb34d752ec','[3rd]', '3rd Person');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('100414b4-8fc0-4f47-b436-24ce54e3d6e3','[ABIL]', 'Abilitave mood');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('ec8aefd4-b33b-4c31-8667-37b43ec3b9c3','[ABL]', 'Ablative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('4e5bb7c3-6d54-4060-bca5-96f735278e95','[ABS]', 'Absolutive');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('c1ace412-ff5e-40c9-8438-9fcdbc58ad88','[ACC]', 'Accusative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('165269c4-46c7-4fd5-a042-81bb961174d1','[ACCOMP]', 'Accompaniment');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('42e9c1c4-01f3-4482-9a70-61e1fa3a7385','[ACT]', 'Active');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('5f897cc8-74c3-48fa-a092-2010a0396610','[ACTR]', 'Actor, transitive agent');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('b11dbe7f-a89c-4ecb-a750-e21973bc0410','[ADJ]', 'Adjective');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('f2f7d655-de95-47ee-8a63-5c760d9e06b5','[ADV]', 'Adverb');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('20761c96-c8ed-47da-8c9d-e959e8d1aded','[ADVBL]', 'Adverbializer');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('43ca3764-3c12-4b47-b97b-032e78c14b54','[ADVRS]', 'Adversative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('36c37b5f-fcdd-4aef-867f-a30643a2d843','[AGR]', 'Agreement');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('2c4102ee-0619-44bb-8d22-08127a5ce067','[AGT]', 'Agent');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('0f3112b4-72c6-41f1-82be-6df1521d65e2','[ALTV]', 'Allative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('1d302510-5c2e-43c4-aa4b-6ad37ab83c82','[ANA]', 'Anaphora');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('ae0b5c1e-7c3b-4155-b1fc-4df0524d44db','[APOS]', 'Apposition');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('7c448df2-766c-4e40-a8d7-4bb0ca5bcfab','[APPL]', 'Applicative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('5f596742-81fd-4100-ab1f-03d3f7f6715a','[ART]', 'Article');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('45d118ce-9d48-4693-8a84-f1f5069d3b05','[ASP]', 'Aspect');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('bd1aef6c-a44f-45b2-a1d7-6720f14be284','[ASSOC]', 'Associative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('cc56670d-36db-48c7-ab3f-36c3c1a83b6a','[ATRB]', 'Attributive ');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('907f7f60-a7be-4527-8b63-2271965cc4c8','[AUX]', 'Auxiliary');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('ee6c76ab-5672-4d2b-b1cd-74dba02076ab','[BEN]', 'Beneficiary, benefactive');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('e61a249e-8cd3-4ed3-b90d-c503f6a37588','[BIAS]', 'Biased word, to discrimiate or injure');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('8a490be5-25b1-4f8a-afb3-1866e58ba78e','[CAT]', 'Category');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('a0a8d57a-80e0-4852-b6df-0d7a388f2489','[CAUS]', 'Causative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('ee187ccd-c1bb-4fcd-8723-946838528a43','[CAUSEE]', 'Causee');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('af646293-4cb8-40df-b10d-bffb86ef95bb','[CLASS]', 'Classifier');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('b414f9a0-02ee-4966-accd-4a024448bb6e','[CLAUS]', 'Clause');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('e2374201-ccf7-44f6-977b-126e7522c1ca','[COMPL]', 'Completive');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('03efdd35-b0f2-4300-98b8-b92aefddbab4','[COMPR]', 'Comparative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('30e05a4c-e5aa-4a2c-b0f5-315f65ea633a','[COMPTZR]', 'Complementizer');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('07155bac-00b2-4527-980f-861981c5851a','[CONCESS]', 'Concessive');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('b5b8ea0f-39b3-411e-82e5-1f450b791a36','[CONJ]', 'Conjuction');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('10de6d42-ca40-4900-81c3-8afe2c3a0f8e','[CONT]', 'Continuous');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('bbf7b7ac-fd51-475a-808d-324a933bfec4','[COP]', 'Copula');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('805d5aa9-02e6-4613-a736-fed31641f491','[DAT]', 'Dative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('ba125a9a-6c1b-4eac-b8ef-7d5667d2b383','[DEB]', 'Debative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('8a7b9115-b8bf-487b-9611-1ceed00adfb2','[DEC-M]', 'Declarative mood');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('cdf1d506-49a8-45d7-bfce-59eb99b87ff5','[DEIC]', 'Deictic');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('05fd69c4-ce95-4766-8233-5c1d87e26861','[DEM]', 'Demonstrative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('f878ce92-5d3e-4ac0-b55e-bfa129406ee4','[DEON]', 'Deontic');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('94c26723-c061-4439-b358-b5789a32f72e','[DESID]', 'Desiderative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('3434a587-7457-4334-9381-a0359a7e0f0e','[DET]', 'Determiner');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('ea8939eb-49b1-4f3e-9f19-abd1f638041b','[DETRANS]', 'Detransitivizer');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('8eac5a31-98d3-4e66-afb0-1b4a3e217629','[DEVERB]', 'Deverbalizer into noun');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('e4214ac8-cd6d-46c1-83ff-025c5321fccf','[DIMIN]', 'Diminutive');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('a33d99e5-1700-4b33-b9ef-4a26fc238799','[DIR]', 'Directional');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('f3e18ba6-dfab-4067-8820-4a06b63d5807','[DIRECT]', 'Direct Knowledge Marker');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('6283f67a-f44b-4f9a-bd35-5d45106fd115','[DIRSPCH]', 'Direct speech act');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('9f73c1d4-fe2f-4c72-b99e-2ce477ef6f8e','[DO]', 'Direct Object');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('5cfe9db2-2ee4-424c-9394-8a153bf41612','[DUAL]', 'Dual');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('9836cb86-4276-4d92-a974-407e23f7f454','[DUB]', 'Dubitative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('ec9e3659-5ee9-49c9-8119-dd2be7d70f1f','[EPIS]', 'Epistemic');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('47fd4706-927b-4020-a6e5-8746fa3bd8e9','[EQUAT]', 'Equative (comparator stating equality)');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('a9c35edf-eac5-42d4-ab7c-6f8caff728e1','[ERG]', 'Ergative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('5ced5d6a-1d3d-470f-885c-973401ccdf97','[EXCL]', 'Exclusive');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('df17b748-b41f-44e1-84ab-ace253eb125a','[EXIST]', 'Existential');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('f12cfa05-e69c-4a79-949a-db8944533840','[EXPR]', 'Experiencer');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('f23de388-6fe1-4f05-873a-9a64a18b58a7','[FEM]', 'Feminine');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('2f319108-1ce0-4991-9173-a816a38ebf70','[FINIT]', 'Finite');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('030773da-984a-4e07-9dfc-6eee05ee03b2','[FOC]', 'Focus');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('ebd77437-4315-4307-b3ea-b69fced60e7d','[GEN]', 'Genitive');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('fffff206-aaa5-420a-9f94-16d4df466a46','[HABIT]', 'Habitual');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('e04f5960-792e-4174-a717-1b35135079d7','[HORT]', 'Hortative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('e3d67f23-9c6e-4ac5-a63f-0ce099d6b3d9','[IDEO]', 'Ideophone');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('a94eeb7f-ecb7-4144-ac86-165a3924bdb7','[IMPERF]', 'Imperfective');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('15f90e6c-05af-4c56-8d2b-bf6cd720567a','[IMPERS]', 'Impersonal');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('63396203-b273-4aaf-858d-d03763974e76','[IMPERV]', 'Imperative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('4e42a66c-4b5b-4011-9165-d2a1a14777dd','[INALIEN]', 'Inalienable');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('1af4252a-b027-4d36-b9df-8606113fb681','[INCEPT]', 'Inceptive');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('ffd6c474-9558-4f1c-b042-fd5fed47b9c3','[INCHOA]', 'Inchoative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('f911a2dd-b782-4656-91fd-30e46f729be1','[INCL]', 'Inclusive');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('9f8182b6-886c-407b-a0a2-e2ae7e4347a1','[INDIRSP]', 'Indirect speech marker');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('f975496a-7c88-43de-8203-bc8fe6b9fa58','[INF]', 'Infinitive');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('85e2d29f-35c6-41d9-81ce-5f3295b9a5dd','[INSTR]', 'Instrumental case');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('f4be6c7a-5808-4761-8472-9ddbeebf1014','[INTENS]', 'Intensifier');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('3e38b54c-cd04-49f3-ba54-83fed6ce0c94','[INTERROG]', 'Interrogative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('d9a8c827-a242-4f7f-92be-551dea46925d','[INTRANS]', 'Intransitive');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('cd5fe046-e256-490a-9a1e-d72adf1d108d','[IO]', 'Indirect object marker');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('7d3483d0-8829-4b5d-9130-c2097ed362c0','[IRREAL]', 'Irrealis');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('e4aae063-1549-4a54-a764-c3834f8f87d7','[ITER]', 'Iterative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('a1bfe850-47c0-40b0-bc5e-7f2c5c8fcb69','[LOC]', 'Locative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('1b694ccd-4131-4fc0-8b6c-1a73cb69f667','[MASC]', 'Masculine');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('88f2948d-b426-44aa-be02-130a6c161374','[MEAS]', 'Measure word');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('ff8a3f64-787f-4be6-9b76-a8b764b5c4b9','[METEOR]', 'Meteorological ');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('e58d8f32-6188-4f6a-94c8-fc06fc42135c','[N-CNT]', 'Count noun');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('b370d242-88ea-4d72-874c-6d0824b8e04e','[N-MASS]', 'Mass noun');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('e2591261-77eb-46a3-a368-68559ec364c4','[N]', 'Noun');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('a1a7dd02-e825-4e75-bf02-47c119319dba','[NEG]', 'Negative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('ae4d7905-43eb-4ab7-80b3-281c1e571e9e','[NEUT]', 'Neuter');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('bf153ee5-f83f-4116-a5ba-5fd680b80ce1','[NOM]', 'Nominative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('b9e80a3a-2b4f-4bdb-8a13-911f13d8affd','[NOMIZR]', 'Nominalizer');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('090f97a7-7636-4ce1-9bc7-831ea389a7b5','[NONFIN]', 'Nonfinite');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('fff3e791-28e1-4708-a0b9-b1670c5dc282','[NONNEG]', 'Non-negative marker');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('750e8922-da10-4610-9781-ef6b039cabbf','[NONPAST]', 'Nonpast tense');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('fe7e3ba8-3a20-46f0-b65f-a0a51a43b58f','[OBJ]', 'Object');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('dd571316-34c6-43fa-bdcd-7c579e4c80e8','[OBJ2]', 'Secondary object');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('701766a3-9ebf-4bc8-9f5f-f8352037767e','[OBLIG]', 'obligatory');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('d13264a1-d00a-43e3-bd94-67b0873a279f','[OPT]', 'Optative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('35e8c4fb-1f7c-4932-b0fa-146259985f6b','[OV]', 'Objective voice');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('bd2692e9-aafd-49d8-a36e-c435c772b149','[PART]', 'Participle');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('1a4121a2-de80-45b7-b24f-7c96593ff040','[PASS]', 'Passive');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('e71a1b68-6ccf-4d49-bcad-0d1c1c35be18','[PAST.HIST]Historical past marker', '');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('5552441d-be11-4529-bb55-b400c671e92a','[Past.Rec]', 'Recent Past');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('a89ab6b9-fc95-4fa2-8888-5bb19611a80a','[PAST]', 'past tense');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('86c8f3ab-410c-45ae-a7d0-9bcf2765e0cd','[PAT]', 'Patient');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('96041501-8c0a-4cdb-9357-7dbcc89878b2','[PAUC]', 'Paucal');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('58e82791-5c19-4e12-b3d5-9d9246b74599','[PERF]', 'Perfect');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('d6b466eb-47f4-4674-baf5-8528b7893170','[PERFV]', 'Perfective');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('406a75fc-e3c7-42dc-8576-b73c33622d4a','[PERMIS]', 'permissive');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('9b39e4ba-b43f-44f6-b50b-4b6dd9ac619a','[PERS]', 'personal name');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('994c67f7-f905-42ef-88a5-6d5fff20b6a1','[PL]', 'Plural');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('62aeb694-8039-4b57-80b3-e039f20904ef','[POSS]', 'Possessor');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('952bc571-58dc-44ae-8704-477574ee1161','[POST]', 'Post-position');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('0cee968d-8617-47dd-83c3-c713a288e478','[PREP]', 'preposition');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('dafea67b-4ce0-4a85-9a5f-9f54b93352be','[PROG]', 'Progressive');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('cc7b8f37-659b-4ae6-be2e-04e972316e38','[PRON]', 'Pronoun');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('8f2de090-0b88-425c-99c0-5c7cb89facf2','[Q-CLS]', 'Closed question');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('b5d93593-1c75-46a0-90b6-0ce79dfff9dd','[Q-CNT]', 'Content question');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('6dbe65da-304b-4ff7-a63d-973512ce8867','[Q-OPN]', 'Open question');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('2548858c-e144-4f2c-9cab-ca9823882409','[Q-RET]', 'Rhetorical question');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('8902191a-acc1-4dc9-a710-0c3de4fe7885','[Q-TAG]', 'Tag question');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('52296811-1a67-4bae-a963-10ddf3e75154','[Q-YN]', 'Yes–no question');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('d686ca50-d2f7-450f-963f-57288bfff4a2','[Q]', 'Question marker');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('11016c91-c846-4ec5-9d92-19529694d28c','[QUANT]', 'Quantifier');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('8ee5a3cb-3618-4d9e-9e97-2d3eaa009787','[REAL]', 'Realis');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('279b52f3-d5e4-49fd-8c1b-dd362564acc0','[RECIP]', 'Recipient');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('5badd8f2-bead-4260-8210-29df6f2e5491','[RECIPR]', 'Reciprocal');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('962d0ac9-a9c7-4201-8e2e-da6bae28c237','[REDUP]', 'Reduplication');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('7376e83d-7f0c-4c7e-a7fa-ec7481c8b970','[REFLEX]', 'Reflexive');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('41bb5021-556f-4bf3-bb4a-837d09688dda','[REL]', 'Relative tense');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('c907ffb3-461f-4381-80f5-903bf01be6a5','[RELZR]', 'Relativizer');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('d4c87f8e-949e-4d3a-a520-1de49668de52','[REPORT]', 'Reportative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('573c6584-a68d-4e1f-89f2-5c73635c8c9b','[RESUM}', 'Resumptive');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('c21ba4a1-8fe2-44ec-83fa-31149dfe5b29','[SING]', 'Singular');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('8fd5cf83-8498-47ca-900e-e3faba939bae','[STAT]', 'Stative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('234c6c38-627a-47eb-97fe-6ffb0ebc63c7','[STIM]', 'Stimulus');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('9b910e6a-951f-401e-9bee-ff83ac835fbe','[SUB]', 'Subject');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('3a0c0863-0c5e-43c2-99e5-3248cc456b14','[SUBJ]', 'Subjunctive');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('b361bcbe-6480-472a-8418-f1cea47df053','[SUPER]', 'Superlative');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('fe0d3648-dcc1-45a1-8ad3-48e2ee5aed0e','[TELIC]', 'Telic');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('b07e9ccc-7108-4910-a3a3-4abcdfdc3f82','[THEME]', 'Theme');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('a7bcf62d-1d5d-4df0-95c0-d42e9647282f','[TOPIC]', 'Topic');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('a5def772-e95f-4871-a0d1-eeac61933940','[TRANS]', 'Transitive');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('687ac1b8-22b4-4378-940e-3f31254d0757','[UNIQ]', 'Uniqueness');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('6afed174-1772-4c1a-8b30-3b40f11d48d1','[V]', 'Verb ');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('b8de99a6-19c1-426b-bc92-891e18ce1f7e','[VALEN-]', 'Valence decreasing');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('d9233322-9e40-4437-8fca-b9c32507642a','[VALEN+]', 'Valence increasing');");
            migrationBuilder.Sql("INSERT INTO Grammar (Id, ShortName, Description) VALUES ('3a1ac100-193e-4b2d-bf9f-b9ce5f39227b','[VERBLZR]', 'Verbalizer');");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
